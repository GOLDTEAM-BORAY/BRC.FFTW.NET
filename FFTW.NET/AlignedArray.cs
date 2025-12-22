#region Copyright and License
/*
This file is part of FFTW.NET, a wrapper around the FFTW library for the .NET framework.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace FFTW.NET;

public unsafe class AlignedArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(int alignment, params int[] lengths) : AbstractPinnedArray<T>(lengths)
    where T : unmanaged
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override unsafe T* Alloc(nuint size)
    {
        return (T*)NativeMemory.AlignedAlloc(size, (nuint)alignment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override unsafe void Free(T* ptr)
    {
        NativeMemory.AlignedFree(ptr);
    }
}

public class AlignedArrayComplex(int alignment, params int[] lengths) : AlignedArray<Complex>(alignment, lengths)
{
    public void MultiplyInPlace(AlignedArrayComplex right)
    {
        var vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>(AsMemory().Span);
        var vectorRight = MemoryMarshal.Cast<Complex, Vector256<double>>(right.AsMemory().Span);
        var matrix = Vector256.Create(1.0, -1.0, 1.0, -1.0);
        for (int i = 0; i < vectorLeft.Length; i++)
        {
            var l = vectorLeft[i];
            var r = vectorRight[i];
            vectorLeft[i] = Avx.HorizontalAdd(
                Avx.Multiply(
                    Avx.Multiply(l, r),
                    matrix),
                Avx.Multiply(
                    l,
                    Avx.Permute(r, 0b0101)
                    ));
        }
        for (int i = 2 * vectorLeft.Length; i < right.Length; i++)
            this[i] = this[i] * right[i];
    }

    public void MultiplyInPlace(Memory<Complex> right)
    {
        var vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>(AsMemory().Span);
        var vectorRight = MemoryMarshal.Cast<Complex, Vector256<double>>(right.Span);
        var matrix = Vector256.Create(1.0, -1.0, 1.0, -1.0);
        for (int i = 0; i < vectorLeft.Length; i++)
        {
            var l = vectorLeft[i];
            var r = vectorRight[i];
            vectorLeft[i] = Avx.HorizontalAdd(
                Avx.Multiply(
                    Avx.Multiply(l, r),
                    matrix),
                Avx.Multiply(
                    l,
                    Avx.Permute(r, 0b0101)
                    ));
        }
        for (int i = 2 * vectorLeft.Length; i < right.Length; i++)
            this[i] = this[i] * right.Span[i];
    }
}

public class AlignedArrayDouble(int alignment, params int[] lengths) : AlignedArray<double>(alignment, lengths)
{
}
