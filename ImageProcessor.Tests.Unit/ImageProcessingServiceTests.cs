using ImageProcessor.Api.Services;
using ImageProcessor.Tests.Unit.Mocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace ImageProcessor.Tests.Unit;

// Unit-тесты изолируют ImageProcessingService от нативной DLL:
// FakeNativeProcessor реализует алгоритмы в managed-коде.
// Проверяется бизнес-логика (PNG decode → фильтр → PNG encode),
// а не корректность C++-кода — это зона ответственности integration-тестов.
public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService _svc = new(new FakeNativeProcessor());

    // Создаёт PNG в памяти из массива пикселей.
    // Не нужен файл на диске — PNG генерируется прямо в тесте.
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
        // Arrange: 1×1, R=100 G=150 B=200 A=200
        using var png = MakePng(1, 1, [new Rgba32(100, 150, 200, 200)]);

        // Act
        byte[] result = await _svc.ProcessAsync(png, "invert");

        // Assert
        using var img = Image.Load<Rgba32>(result);
        var p = img[0, 0];
        Assert.Equal(155, p.R); // 255 - 100
        Assert.Equal(105, p.G); // 255 - 150
        Assert.Equal(55,  p.B); // 255 - 200
        Assert.Equal(200, p.A); // не тронут
    }

    [Fact]
    public async Task ProcessAsync_GrayscaleFilter_SetsRgbToLuminanceBt601()
    {
        // Arrange: R=200 G=100 B=50 A=255
        // Ожидаемая яркость BT.601: 0.299*200 + 0.587*100 + 0.114*50 = 124.2 → 124
        using var png = MakePng(1, 1, [new Rgba32(200, 100, 50, 255)]);

        // Act
        byte[] result = await _svc.ProcessAsync(png, "grayscale");

        // Assert
        using var img = Image.Load<Rgba32>(result);
        var p = img[0, 0];
        Assert.Equal(p.R, p.G); // R = G = B (серый)
        Assert.Equal(p.G, p.B);
        Assert.InRange(p.R, 123, 125); // ±1 на случай разницы в округлении
        Assert.Equal(255, p.A);
    }

    [Fact]
    public async Task ProcessAsync_BlurFilter_BlursCenterPixelTowardNeighborAverage()
    {
        // Arrange: 3×3, все пиксели R=0 кроме центра (1,1) R=255
        var pixels = Enumerable.Repeat(new Rgba32(0, 0, 0, 255), 9).ToArray();
        pixels[4] = new Rgba32(255, 0, 0, 255); // центр
        using var png = MakePng(3, 3, pixels);

        // Act: 1 итерация
        byte[] result = await _svc.ProcessAsync(png, "blur", blurIterations: 1);

        // Assert: центральный пиксель стал средним (255 + 8*0) / 9 = 28 ± 3
        using var img = Image.Load<Rgba32>(result);
        var center = img[1, 1];
        Assert.InRange(center.R, 25, 31);
        Assert.Equal(255, center.A); // alpha не тронут
    }

    [Fact]
    public async Task ProcessAsync_UnknownFilter_ThrowsArgumentException()
    {
        using var png = MakePng(1, 1, [new Rgba32(100, 100, 100, 255)]);
        await Assert.ThrowsAsync<ArgumentException>(
            () => _svc.ProcessAsync(png, "unknown"));
    }
}
