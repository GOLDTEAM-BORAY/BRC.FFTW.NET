#region Copyright and License
/*
This file is part of FFTW.NET, a wrapper around the FFTW library
for the .NET framework.
Copyright (C) 2017 Tobias Meyer

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/
#endregion

using System.Runtime.InteropServices;
using System.Text;

namespace FFTW.NET
{
    public enum DftDirection : int
    {
        Forwards = -1,
        Backwards = 1
    }

    [Flags]
    public enum PlannerFlags : uint
    {
        Default = Measure,

        /// <summary>
        /// <see cref="Measure"/> tells FFTW to find an optimized plan by actually
        /// computing several FFTs and measuring their execution time.
        /// Depending on your machine, this can take some time (often a few seconds).
        /// </summary>
        Measure = (0U),

        /// <summary>
        /// <see cref="Exhaustive"/> is like <see cref="Patient"/>,
        /// but considers an even wider range of algorithms,
        /// including many that we think are unlikely to be fast,
        /// to produce the most optimal plan but with a substantially increased planning time. 
        /// </summary>
        Exhaustive = (1U << 3),

        /// <summary>
        /// <see cref="Patient"/> is like <see cref="Measure"/>,
        /// but considers a wider range of algorithms and often produces
        /// a “more optimal” plan (especially for large transforms),
        /// but at the expense of several times longer planning time
        /// (especially for large transforms). 
        /// </summary>
        Patient = (1U << 5),

        /// <summary>
        /// <see cref="Estimate"/> specifies that,
        /// instead of actual measurements of different algorithms,
        /// a simple heuristic is used to pick a (probably sub-optimal) plan quickly.
        /// With this flag, the input/output arrays are not overwritten during planning.
        /// </summary>
        Estimate = (1U << 6),

        /// <summary>
        /// <see cref="WisdomOnly"/> is a special planning mode in which
        /// the plan is only created if wisdom is available for the given problem,
        /// and otherwise a <c>null</c> plan is returned. This can be combined
        /// with other flags, e.g. '<see cref="WisdomOnly"/> | <see cref="Patient"/>'
        /// creates a plan only if wisdom is available that was created in
        /// <see cref="Patient"/> or <see cref="Exhaustive"/> mode.
        /// The <see cref="WisdomOnly"/> flag is intended for users who need to
        /// detect whether wisdom is available; for example, if wisdom is not
        /// available one may wish to allocate new arrays for planning so that
        /// user data is not overwritten. 
        /// </summary>
        WisdomOnly = (1U << 21)
    }

    public static partial class FftwInterop
    {
        static readonly Version _version = GetVersionAndInitialize();

        public static Version Version => _version;

        public static bool IsAvailable => _version != null;

        internal static object Lock => _version;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WriteCharHandler(byte c, IntPtr ptr);

        static Version GetVersionAndInitialize()
        {
            try { fftw_init_threads(); }
            catch (DllNotFoundException)
            {
                return null;
            }

            string version = GetVersion();
            return new Version(version);
        }

        static string GetVersion()
        {
            const string VersionPrefix = "fftw-";
            const byte WhiteSpace = (byte)' ';
            byte[] prefix = Encoding.ASCII.GetBytes(VersionPrefix);
            int i = 0;
            StringBuilder sb = new StringBuilder();
            void writeChar(byte c, nint ptr)
            {
                if (i < 0)
                    return;

                if (i == VersionPrefix.Length)
                {
                    if (c == WhiteSpace)
                        i = -1;
                    else
                        sb.Append((char)c);
                }
                else if (c == prefix[i])
                    i++;
                else
                    i = 0;
            }
            // This is only called on initialization, so no synchronization/lock is required
            fftw_export_wisdom(writeChar, IntPtr.Zero);

            return sb.ToString();
        }




        const string DLL_PATH = "fftw3";


        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool fftw_init_threads();

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_plan_with_nthreads(int nthreads);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial IntPtr fftw_plan_dft(int rank, [MarshalAs(UnmanagedType.LPArray)] int[] n, IntPtr arrIn, IntPtr arrOut, DftDirection direction, PlannerFlags flags);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial IntPtr fftw_plan_dft_r2c(int rank, [MarshalAs(UnmanagedType.LPArray)] int[] n, IntPtr arrIn, IntPtr arrOut, PlannerFlags flags);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial IntPtr fftw_plan_dft_c2r(int rank, [MarshalAs(UnmanagedType.LPArray)] int[] n, IntPtr arrIn, IntPtr arrOut, PlannerFlags flags);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_destroy_plan(IntPtr plan);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_execute(IntPtr plan);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool fftw_export_wisdom_to_filename([MarshalAs(UnmanagedType.LPStr)] string filename);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool fftw_import_wisdom_from_filename([MarshalAs(UnmanagedType.LPStr)] string filename);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static partial string fftw_export_wisdom_to_string();

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool fftw_import_wisdom_from_string([MarshalAs(UnmanagedType.LPStr)] string wisdom);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_export_wisdom([MarshalAs(UnmanagedType.FunctionPtr)] WriteCharHandler writeChar, IntPtr data);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_forget_wisdom();

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial IntPtr fftw_malloc(IntPtr size);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial void fftw_free(IntPtr ptr);

        [LibraryImport(DLL_PATH)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        public static partial int fftw_alignment_of(IntPtr ptr);
    }
}
