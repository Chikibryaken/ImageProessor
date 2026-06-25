using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace ImageProcessor.Api.Services;

public sealed class ImageProcessingService
{
    private readonly INativeImageProcessor _native;

    public ImageProcessingService(INativeImageProcessor native)
    {
        _native = native;
    }

    public string TestNativeCall()
    {
        byte[] pixels = [255, 0, 0, 255];
        _native.InvertColors(pixels, pixels.Length);
        return $"OK: R={pixels[0]} G={pixels[1]} B={pixels[2]} A={pixels[3]} (ожидается 0 255 255 255)";
    }

    public async Task<byte[]> ProcessAsync(Stream imageStream, string filter, int blurIterations = 5)
    {
        using var image = await Image.LoadAsync<Rgba32>(imageStream);
        ApplyFilter(image, filter, blurIterations);
        using var output = new MemoryStream();
        await image.SaveAsPngAsync(output);
        return output.ToArray();
    }

    private void ApplyFilter(Image<Rgba32> image, string filter, int blurIterations)
    {
        if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelMemory))
            throw new InvalidOperationException(
                "ImageSharp не смог предоставить непрерывный блок памяти для пикселей.");

        Span<byte> pixelBytes = MemoryMarshal.AsBytes(pixelMemory.Span);
        byte[] pixels = pixelBytes.ToArray();

        switch (filter.ToLowerInvariant())
        {
            case "invert":
                _native.InvertColors(pixels, pixels.Length);
                break;
            case "grayscale":
                _native.Grayscale(pixels, pixels.Length);
                break;
            case "blur":
                _native.Blur(pixels, image.Width, image.Height, blurIterations);
                break;
            default:
                throw new ArgumentException($"Неизвестный фильтр: '{filter}'.", nameof(filter));
        }

        new ReadOnlySpan<byte>(pixels).CopyTo(pixelBytes);
    }
}
