using System.Runtime.InteropServices;

namespace ImageProcessor.Api.Services;

public sealed class NativeImageProcessor : INativeImageProcessor
{
    private static class NativeMethods
    {
        [DllImport("ImageProcessor.Native", EntryPoint = "InvertColors",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void InvertColors([In, Out] byte[] pixels, int length);

        [DllImport("ImageProcessor.Native", EntryPoint = "Grayscale",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Grayscale([In, Out] byte[] pixels, int length);

        [DllImport("ImageProcessor.Native", EntryPoint = "Blur",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Blur([In, Out] byte[] pixels, int width, int height, int iterations);
    }

    public void InvertColors(byte[] pixels, int length) => NativeMethods.InvertColors(pixels, length);
    public void Grayscale(byte[] pixels, int length)    => NativeMethods.Grayscale(pixels, length);

    public void Blur(byte[] pixels, int width, int height, int iterations) =>
        NativeMethods.Blur(pixels, width, height, iterations);
}
