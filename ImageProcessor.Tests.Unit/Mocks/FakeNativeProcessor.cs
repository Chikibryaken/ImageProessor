using ImageProcessor.Api.Services;

namespace ImageProcessor.Tests.Unit.Mocks;

// Managed-реализация тех же алгоритмов что и в C++ DLL.
// Не требует .dll, работает на любой платформе/CI.
// Намеренно дублирует логику — если алгоритм изменится в C++
// и тест начнёт падать, это сигнал проверить совместимость.
public sealed class FakeNativeProcessor : INativeImageProcessor
{
    public void InvertColors(byte[] pixels, int length)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            pixels[i]     = (byte)(255 - pixels[i]);
            pixels[i + 1] = (byte)(255 - pixels[i + 1]);
            pixels[i + 2] = (byte)(255 - pixels[i + 2]);
            // alpha не трогаем
        }
    }

    public void Grayscale(byte[] pixels, int length)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            // BT.601: те же коэффициенты что и в C++
            byte gray = (byte)(0.299 * pixels[i] + 0.587 * pixels[i + 1] + 0.114 * pixels[i + 2]);
            pixels[i] = pixels[i + 1] = pixels[i + 2] = gray;
        }
    }

    public void Blur(byte[] pixels, int width, int height, int iterations)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            byte[] src = (byte[])pixels.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sumR = 0, sumG = 0, sumB = 0;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = Math.Clamp(x + dx, 0, width - 1);
                            int ny = Math.Clamp(y + dy, 0, height - 1);
                            int idx = (ny * width + nx) * 4;
                            sumR += src[idx];
                            sumG += src[idx + 1];
                            sumB += src[idx + 2];
                        }
                    }

                    int i = (y * width + x) * 4;
                    pixels[i]     = (byte)(sumR / 9);
                    pixels[i + 1] = (byte)(sumG / 9);
                    pixels[i + 2] = (byte)(sumB / 9);
                    // alpha не трогаем
                }
            }
        }
    }
}
