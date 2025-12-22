using System.Buffers;

namespace FFTW.NET;

internal unsafe class UnmanagedMemoryManager<T>(T* ptr, int length) : MemoryManager<T> where T : unmanaged
{
    public unsafe override Span<T> GetSpan()
    {
        return new Span<T>(ptr, length);
    }

    public unsafe override MemoryHandle Pin(int elementIndex = 0)
    {
        // 非托管内存已固定，直接返回指针
        return new MemoryHandle(ptr + elementIndex);
    }

    public override void Unpin()
    {
        // 非托管内存无需手动 Unpin
    }

    protected override void Dispose(bool disposing)
    {
        // 使用场景都是外部创建的指针，此处不释放非托管内存
        //Marshal.FreeHGlobal(_ptr);
    }
}
