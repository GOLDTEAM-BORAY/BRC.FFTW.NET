using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFTW.NET
{
    public unsafe abstract class AbstractPinnedArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T> : IPinnedArray<T>
        where T : unmanaged
    {
        private readonly T* _buffer;
        readonly int _length;
        readonly int[] _lengths;

        public int Length => _length;
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;
        public int Rank => _lengths.Length;

        public IntPtr Pointer => new(_buffer);

        public Memory<T> AsMemory() => new UnmanagedMemoryManager<T>(_buffer, _length).Memory;

        public AbstractPinnedArray(params int[] lengths)
        {
            _length = Utils.GetTotalSize(lengths);
            _lengths = lengths;

            var length = Marshal.SizeOf<T>();
            foreach (var n in lengths)
                length *= n;
            _buffer = Alloc((nuint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual T* Alloc(nuint size)
        {
            return (T*)NativeMemory.Alloc(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Free(T* ptr)
        {
            NativeMemory.Free(ptr);
        }


        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                Free(_buffer);
                _isDisposed = true;
            }
        }

        ~AbstractPinnedArray()
        {
            Dispose();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLength(int dimension) => _lengths[dimension];

        public int[] GetSize()
        {
            int[] result = new int[Rank];
            Buffer.BlockCopy(_lengths, 0, result, 0, Rank * sizeof(int));
            return result;
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
    }
}
