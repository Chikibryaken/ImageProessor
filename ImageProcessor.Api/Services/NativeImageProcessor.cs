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

        [DllImport("ImageProcessor.Native", EntryPoint = "Brightness",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Brightness([In, Out] byte[] pixels, int length, float factor);

        [DllImport("ImageProcessor.Native", EntryPoint = "Contrast",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Contrast([In, Out] byte[] pixels, int length, float factor);

        [DllImport("ImageProcessor.Native", EntryPoint = "Sepia",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sepia([In, Out] byte[] pixels, int length);

        [DllImport("ImageProcessor.Native", EntryPoint = "Sharpen",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sharpen([In, Out] byte[] pixels, int width, int height);

        [DllImport("ImageProcessor.Native", EntryPoint = "Pixelate",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pixelate([In, Out] byte[] pixels, int width, int height, int blockSize);

        [DllImport("ImageProcessor.Native", EntryPoint = "FlipHorizontal",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlipHorizontal([In, Out] byte[] pixels, int width, int height);

        [DllImport("ImageProcessor.Native", EntryPoint = "FlipVertical",
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlipVertical([In, Out] byte[] pixels, int width, int height);
    }

    public void InvertColors(byte[] pixels, int length)  => NativeMethods.InvertColors(pixels, length);
    public void Grayscale(byte[] pixels, int length)     => NativeMethods.Grayscale(pixels, length);
    public void Blur(byte[] pixels, int width, int height, int iterations) =>
        NativeMethods.Blur(pixels, width, height, iterations);
    public void Brightness(byte[] pixels, int length, float factor) =>
        NativeMethods.Brightness(pixels, length, factor);
    public void Contrast(byte[] pixels, int length, float factor) =>
        NativeMethods.Contrast(pixels, length, factor);
    public void Sepia(byte[] pixels, int length)         => NativeMethods.Sepia(pixels, length);
    public void Sharpen(byte[] pixels, int width, int height) =>
        NativeMethods.Sharpen(pixels, width, height);
    public void Pixelate(byte[] pixels, int width, int height, int blockSize) =>
        NativeMethods.Pixelate(pixels, width, height, blockSize);
    public void FlipHorizontal(byte[] pixels, int width, int height) =>
        NativeMethods.FlipHorizontal(pixels, width, height);
    public void FlipVertical(byte[] pixels, int width, int height) =>
        NativeMethods.FlipVertical(pixels, width, height);
}
