#region Copyright and License
/*
This file is part of FFTW.NET, a wrapper around the FFTW library for the .NET framework.
Copyright (C) 2017 Tobias Meyer
License: Microsoft Reciprocal License (MS-RL)
*/
#endregion

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace FFTW.NET;

public unsafe class AlignedArray<T> : MemoryManager<T>, IPinnedArray<T>
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

    public override Span<T> GetSpan() => new(_buffer, _length);

    public AlignedArray(int alignment, params int[] lengths)
    {
        _length = Utils.GetTotalSize(lengths);
        _lengths = lengths;

        var length = Marshal.SizeOf<T>();
        foreach (var n in lengths)
            length *= n;
        _buffer = (T*)NativeMemory.AlignedAlloc((nuint)length, (nuint)alignment);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            NativeMemory.AlignedFree(_buffer);
            _isDisposed = true;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetLength(int dimension) => _lengths[dimension];

    public int[] GetSize()
    {
        int[] result = new int[Rank];
        Buffer.BlockCopy(_lengths, 0, result, 0, Rank * sizeof(int));
        return result;
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

public class AlignedArrayComplex(int alignment, params int[] lengths) : AlignedArray<Complex>(alignment, lengths)
{
    public void MultiplyInPlace(Memory<Complex> right)
    {
        var vectorLeft = MemoryMarshal.Cast<Complex, Vector256<double>>(GetSpan());
        var vectorRight = MemoryMarshal.Cast<Complex, Vector256<double>>(right.Span);
        var matrix = Vector256.Create(1.0, -1.0, 1.0, -1.0);
        for (int i = 0; i < vectorLeft.Length; i++)
        {
            var l = vectorLeft[i];
            var r = vectorRight[i];
            vectorLeft[i] = Avx.HorizontalAdd(
                Avx.Multiply(Avx.Multiply(l, r), matrix),
                Avx.Multiply(l, Avx.Permute(r, 0b0101)));
        }
        for (int i = 2 * vectorLeft.Length; i < right.Length; i++)
            this[i] = this[i] * right.Span[i];
    }
    public void MultiplyInPlace(AlignedArrayComplex right)
    {
        MultiplyInPlace(right.Memory);
    }
}

public class FftwArrayComplex(params int[] lengths) : AlignedArrayComplex(Marshal.SizeOf<Complex>(), lengths)
{
}

public class AlignedArrayDouble(int alignment, params int[] lengths) : AlignedArray<double>(alignment, lengths)
{
}
