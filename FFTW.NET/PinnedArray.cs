#region Copyright and License
/*
This file is part of FFTW.NET, a wrapper around the FFTW library for the .NET framework.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFTW.NET
{
    /// <summary>
    /// Wrapper around <see cref="System.Array"/> which provides some level of type-safety.
    /// If you have access to the strongly typed instance of the underlying buffer (e.g. T[], T[,], T[,,], etc.)
    /// you should always get and set values using the strongly typed instance. <see cref="Array{T}"/>
    /// uses <see cref="Array.SetValue"/>/<see cref="Array.GetValue"/> internally which is much slower than
    /// using the strongly typed instance directly.
    /// </summary>
    public unsafe class PinnedArray<T> : MemoryManager<T>, IPinnedArray<T> where T : unmanaged
    {
        readonly GCHandle? _pin;
        readonly MemoryHandle? _memoryHandle;
        readonly IMemoryOwner<T> _memoryOwner;

        private readonly T* _buffer;
        readonly int _length;
        readonly int[] _lengths;

        public int Length => _length;
        public int Rank => _lengths.Length;
        public IntPtr Pointer => new(_buffer);
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;


        public PinnedArray(params int[] lengths)
        {
            _length = Utils.GetTotalSize(lengths);
            _lengths = lengths;
            _memoryOwner = MemoryPool<T>.Shared.Rent(_length);
            _memoryHandle = _memoryOwner.Memory.Pin();
            _buffer = (T*)_memoryHandle.Value.Pointer;
        }

        public PinnedArray(Array array)
        {
            if (array.GetType().GetElementType() != typeof(T))
                throw new ArgumentException($"Must have elements of type {typeof(T).FullName}.", nameof(array));

            _pin = GCHandle.Alloc(array, GCHandleType.Pinned);

            _length = array.Length;

            _lengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
            {
                _lengths[i] = array.GetLength(i);
            }

            _buffer = (T*)_pin.Value.AddrOfPinnedObject().ToPointer();
        }

        public override Span<T> GetSpan() => new(_buffer, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLength(int dimension) => _lengths[dimension];

        public int[] GetSize()
        {
            int[] result = new int[Rank];
            Buffer.BlockCopy(_lengths, 0, result, 0, Rank * sizeof(int));
            return result;
        }
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _pin?.Free();
                _memoryHandle?.Dispose();
                _memoryOwner?.Dispose();
                _isDisposed = true;
            }
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            throw new NotSupportedException();
        }

        public override void Unpin()
        {
            throw new NotSupportedException();
        }

        public T this[int i1]
        {
            get => *(_buffer + i1);
            set => *(_buffer + i1) = value;
        }

        public T this[int i1, int i2]
        {
            get => *(_buffer + (i2 + _lengths[1] * i1));
            set => *(_buffer + (i2 + _lengths[1] * i1)) = value;
        }

        public T this[int i1, int i2, int i3]
        {
            get => *(_buffer + (i3 + _lengths[2] * (i2 + _lengths[1] * i1)));
            set => *(_buffer + (i3 + _lengths[2] * (i2 + _lengths[1] * i1))) = value;
        }

        public T this[params int[] indices]
        {
            get => *(_buffer + this.GetIndex(indices));
            set => *(_buffer + this.GetIndex(indices)) = value;
        }

        public static explicit operator PinnedArray<T>(Array array) => new PinnedArray<T>(array);

    }
}