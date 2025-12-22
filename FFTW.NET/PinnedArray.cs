using System.Diagnostics.CodeAnalysis;

namespace FFTW.NET
{
    public unsafe sealed class PinnedArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(params int[] lengths) : AbstractPinnedArray<T>(lengths)
          where T : unmanaged
    {
    }
}
