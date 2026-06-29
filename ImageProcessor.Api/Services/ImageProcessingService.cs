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

    public async Task<byte[]> ProcessAsync(
        Stream imageStream, string filter, float intensity = 1.0f, int blurIterations = 5)
    {
        using var image = await Image.LoadAsync<Rgba32>(imageStream);
        ApplyFilter(image, filter, intensity, blurIterations);
        using var output = new MemoryStream();
        await image.SaveAsPngAsync(output);
        return output.ToArray();
    }

    private void ApplyFilter(Image<Rgba32> image, string filter, float intensity, int blurIterations)
    {
        int stride = image.Width * 4;
        byte[] pixels = new byte[image.Height * stride];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
                MemoryMarshal.AsBytes(accessor.GetRowSpan(y))
                             .CopyTo(pixels.AsSpan(y * stride));
        });

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
            case "brightness":
                _native.Brightness(pixels, pixels.Length, intensity);
                break;
            case "contrast":
                _native.Contrast(pixels, pixels.Length, intensity);
                break;
            case "sepia":
                _native.Sepia(pixels, pixels.Length);
                break;
            case "sharpen":
                int sharpenLevel = (int)intensity;
                if (sharpenLevel > 0)
                    for (int i = 0; i < Math.Min(sharpenLevel, 5); i++)
                        _native.Sharpen(pixels, image.Width, image.Height);
                else if (sharpenLevel < 0)
                    _native.Blur(pixels, image.Width, image.Height, Math.Min(-sharpenLevel, 5));
                break;
            case "pixelate":
                _native.Pixelate(pixels, image.Width, image.Height, (int)intensity);
                break;
            case "flip-horizontal":
                _native.FlipHorizontal(pixels, image.Width, image.Height);
                break;
            case "flip-vertical":
                _native.FlipVertical(pixels, image.Width, image.Height);
                break;
            default:
                throw new ArgumentException($"Неизвестный фильтр: '{filter}'.", nameof(filter));
        }

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
                pixels.AsSpan(y * stride, stride)
                      .CopyTo(MemoryMarshal.AsBytes(accessor.GetRowSpan(y)));
        });
    }
}
