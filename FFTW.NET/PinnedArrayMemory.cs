using System.Buffers;
using System.Runtime.CompilerServices;

namespace FFTW.NET;

public unsafe class PinnedArrayMemory<T> : MemoryManager<T>, IPinnedArray<T> where T : unmanaged
{
    readonly IMemoryOwner<T> _memoryOwner;
    readonly MemoryHandle? _memoryHandle;

    private readonly T* _buffer;
    readonly int _length;
    readonly int[] _lengths;

    public int Length => _length;
    public int Rank => _lengths.Length;
    public IntPtr Pointer => new(_buffer);
    private bool _isDisposed;
    public bool IsDisposed => _isDisposed;


    public PinnedArrayMemory(Memory<T> memory)
    {
        _memoryHandle = memory.Pin();
        _length = memory.Length;
        _lengths = [memory.Length];
        _buffer = (T*)_memoryHandle.Value.Pointer;
    }

    public PinnedArrayMemory(ReadOnlyMemory<T> memory)
    {
        _memoryHandle = memory.Pin();
        _length = memory.Length;
        _lengths = [memory.Length];
        _buffer = (T*)_memoryHandle.Value.Pointer;
    }

    public PinnedArrayMemory(params int[] lengths)
    {
        _length = Utils.GetTotalSize(lengths);
        _lengths = lengths;
        _memoryOwner = MemoryPool<T>.Shared.Rent(_length);
        _memoryHandle = _memoryOwner.Memory.Pin();
        _buffer = (T*)_memoryHandle.Value.Pointer;
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
}
