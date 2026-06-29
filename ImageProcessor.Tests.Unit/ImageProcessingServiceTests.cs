using ImageProcessor.Api.Services;
using ImageProcessor.Tests.Unit.Mocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace ImageProcessor.Tests.Unit;

public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _svc = new(new FakeNativeProcessor());

    private static Stream MakePng(int width, int height, Rgba32[] pixelData)
    {
        var image = new Image<Rgba32>(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                image[x, y] = pixelData[y * width + x];
        var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task ProcessAsync_InvertFilter_InvertsRgbAndPreservesAlpha()
    {
        using var png = MakePng(1, 1, [new Rgba32(100, 150, 200, 200)]);

        byte[] result = await _svc.ProcessAsync(png, "invert");

        using var img = Image.Load<Rgba32>(result);
        var p = img[0, 0];
        Assert.Equal(155, p.R);
        Assert.Equal(105, p.G);
        Assert.Equal(55,  p.B);
        Assert.Equal(200, p.A);
    }

    [Fact]
    public async Task ProcessAsync_GrayscaleFilter_SetsRgbToLuminanceBt601()
    {
        using var png = MakePng(1, 1, [new Rgba32(200, 100, 50, 255)]);

        byte[] result = await _svc.ProcessAsync(png, "grayscale");

        using var img = Image.Load<Rgba32>(result);
        var p = img[0, 0];
        Assert.Equal(p.R, p.G);
        Assert.Equal(p.G, p.B);
        Assert.InRange(p.R, 123, 125);
        Assert.Equal(255, p.A);
    }

    [Fact]
    public async Task ProcessAsync_BlurFilter_BlursCenterPixelTowardNeighborAverage()
    {
        var pixels = Enumerable.Repeat(new Rgba32(0, 0, 0, 255), 9).ToArray();
        pixels[4] = new Rgba32(255, 0, 0, 255);
        using var png = MakePng(3, 3, pixels);

        byte[] result = await _svc.ProcessAsync(png, "blur", blurIterations: 1);

        using var img = Image.Load<Rgba32>(result);
        var center = img[1, 1];
        Assert.InRange(center.R, 25, 31);
        Assert.Equal(255, center.A);
    }

    [Fact]
    public async Task ProcessAsync_UnknownFilter_ThrowsArgumentException()
    {
        using var png = MakePng(1, 1, [new Rgba32(100, 100, 100, 255)]);
        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.ProcessAsync(png, "unknown"));
    }
}
