using ImageProcessor.Api.Services;

namespace ImageProcessor.Tests.Unit.Mocks;

public sealed class FakeNativeProcessor : INativeImageProcessor
{
    private static byte ClampByte(float v) =>
        (byte)(v < 0 ? 0 : v > 255 ? 255 : v);

    public void InvertColors(byte[] pixels, int length)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            pixels[i]     = (byte)(255 - pixels[i]);
            pixels[i + 1] = (byte)(255 - pixels[i + 1]);
            pixels[i + 2] = (byte)(255 - pixels[i + 2]);
        }
    }

    public void Grayscale(byte[] pixels, int length)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
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
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = Math.Clamp(x + dx, 0, width - 1);
                            int ny = Math.Clamp(y + dy, 0, height - 1);
                            int idx = (ny * width + nx) * 4;
                            sumR += src[idx];
                            sumG += src[idx + 1];
                            sumB += src[idx + 2];
                        }

                    int i = (y * width + x) * 4;
                    pixels[i]     = (byte)(sumR / 9);
                    pixels[i + 1] = (byte)(sumG / 9);
                    pixels[i + 2] = (byte)(sumB / 9);
                }
            }
        }
    }

    public void Brightness(byte[] pixels, int length, float factor)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            pixels[i]     = ClampByte(pixels[i]     * factor);
            pixels[i + 1] = ClampByte(pixels[i + 1] * factor);
            pixels[i + 2] = ClampByte(pixels[i + 2] * factor);
        }
    }

    public void Contrast(byte[] pixels, int length, float factor)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            pixels[i]     = ClampByte(128f + (pixels[i]     - 128f) * factor);
            pixels[i + 1] = ClampByte(128f + (pixels[i + 1] - 128f) * factor);
            pixels[i + 2] = ClampByte(128f + (pixels[i + 2] - 128f) * factor);
        }
    }

    public void Sepia(byte[] pixels, int length)
    {
        for (int i = 0; i + 3 < length; i += 4)
        {
            float r = pixels[i], g = pixels[i + 1], b = pixels[i + 2];
            pixels[i]     = ClampByte(r * 0.393f + g * 0.769f + b * 0.189f);
            pixels[i + 1] = ClampByte(r * 0.349f + g * 0.686f + b * 0.168f);
            pixels[i + 2] = ClampByte(r * 0.272f + g * 0.534f + b * 0.131f);
        }
    }

    public void Sharpen(byte[] pixels, int width, int height)
    {
        byte[] src = (byte[])pixels.Clone();

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int c = 0; c < 3; c++)
                {
                    int idx = (y * width + x) * 4 + c;
                    float val = 5f * src[idx];
                    if (y > 0)           val -= src[((y - 1) * width + x) * 4 + c];
                    if (y < height - 1)  val -= src[((y + 1) * width + x) * 4 + c];
                    if (x > 0)           val -= src[(y * width + (x - 1)) * 4 + c];
                    if (x < width - 1)   val -= src[(y * width + (x + 1)) * 4 + c];
                    pixels[idx] = ClampByte(val);
                }
    }

    public void Pixelate(byte[] pixels, int width, int height, int blockSize)
    {
        if (blockSize < 1) blockSize = 1;

        for (int by = 0; by < height; by += blockSize)
            for (int bx = 0; bx < width; bx += blockSize)
            {
                int sumR = 0, sumG = 0, sumB = 0, count = 0;
                int yEnd = Math.Min(by + blockSize, height);
                int xEnd = Math.Min(bx + blockSize, width);

                for (int y = by; y < yEnd; y++)
                    for (int x = bx; x < xEnd; x++)
                    {
                        int idx = (y * width + x) * 4;
                        sumR += pixels[idx];
                        sumG += pixels[idx + 1];
                        sumB += pixels[idx + 2];
                        count++;
                    }

                byte avgR = (byte)(sumR / count);
                byte avgG = (byte)(sumG / count);
                byte avgB = (byte)(sumB / count);

                for (int y = by; y < yEnd; y++)
                    for (int x = bx; x < xEnd; x++)
                    {
                        int idx = (y * width + x) * 4;
                        pixels[idx]     = avgR;
                        pixels[idx + 1] = avgG;
                        pixels[idx + 2] = avgB;
                    }
            }
    }

    public void FlipHorizontal(byte[] pixels, int width, int height)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width / 2; x++)
            {
                int idxL = (y * width + x) * 4;
                int idxR = (y * width + (width - 1 - x)) * 4;
                for (int c = 0; c < 4; c++)
                    (pixels[idxL + c], pixels[idxR + c]) = (pixels[idxR + c], pixels[idxL + c]);
            }
    }

    public void FlipVertical(byte[] pixels, int width, int height)
    {
        for (int y = 0; y < height / 2; y++)
            for (int x = 0; x < width; x++)
            {
                int idxT = (y * width + x) * 4;
                int idxB = ((height - 1 - y) * width + x) * 4;
                for (int c = 0; c < 4; c++)
                    (pixels[idxT + c], pixels[idxB + c]) = (pixels[idxB + c], pixels[idxT + c]);
            }
    }
}
