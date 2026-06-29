#include "image_ops.h"
#include <vector>
#include <algorithm>

static inline unsigned char clampByte(float v)
{
    return static_cast<unsigned char>(v < 0.0f ? 0 : v > 255.0f ? 255 : v);
}

extern "C" EXPORT void InvertColors(unsigned char* pixels, int length)
{
    for (int i = 0; i + 3 < length; i += 4)
    {
        pixels[i]     = static_cast<unsigned char>(255 - pixels[i]);
        pixels[i + 1] = static_cast<unsigned char>(255 - pixels[i + 1]);
        pixels[i + 2] = static_cast<unsigned char>(255 - pixels[i + 2]);
    }
}

extern "C" EXPORT void Grayscale(unsigned char* pixels, int length)
{
    for (int i = 0; i + 3 < length; i += 4)
    {
        auto gray = static_cast<unsigned char>(
            0.299 * pixels[i] + 0.587 * pixels[i + 1] + 0.114 * pixels[i + 2]);
        pixels[i]     = gray;
        pixels[i + 1] = gray;
        pixels[i + 2] = gray;
    }
}

extern "C" EXPORT void Blur(unsigned char* pixels, int width, int height, int iterations)
{
    if (iterations < 1) iterations = 1;

    const int length = width * height * 4;
    std::vector<unsigned char> src(length);

    for (int iter = 0; iter < iterations; ++iter)
    {
        src.assign(pixels, pixels + length);

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int sumR = 0, sumG = 0, sumB = 0;

                for (int dy = -1; dy <= 1; ++dy)
                {
                    for (int dx = -1; dx <= 1; ++dx)
                    {
                        int nx = x + dx < 0 ? 0 : (x + dx >= width  ? width  - 1 : x + dx);
                        int ny = y + dy < 0 ? 0 : (y + dy >= height ? height - 1 : y + dy);
                        int idx = (ny * width + nx) * 4;
                        sumR += src[idx];
                        sumG += src[idx + 1];
                        sumB += src[idx + 2];
                    }
                }

                int idx = (y * width + x) * 4;
                pixels[idx]     = static_cast<unsigned char>(sumR / 9);
                pixels[idx + 1] = static_cast<unsigned char>(sumG / 9);
                pixels[idx + 2] = static_cast<unsigned char>(sumB / 9);
            }
        }
    }
}

extern "C" EXPORT void Brightness(unsigned char* pixels, int length, float factor)
{
    for (int i = 0; i + 3 < length; i += 4)
    {
        pixels[i]     = clampByte(pixels[i]     * factor);
        pixels[i + 1] = clampByte(pixels[i + 1] * factor);
        pixels[i + 2] = clampByte(pixels[i + 2] * factor);
    }
}

extern "C" EXPORT void Contrast(unsigned char* pixels, int length, float factor)
{
    for (int i = 0; i + 3 < length; i += 4)
    {
        pixels[i]     = clampByte(128.0f + (pixels[i]     - 128.0f) * factor);
        pixels[i + 1] = clampByte(128.0f + (pixels[i + 1] - 128.0f) * factor);
        pixels[i + 2] = clampByte(128.0f + (pixels[i + 2] - 128.0f) * factor);
    }
}

extern "C" EXPORT void Sepia(unsigned char* pixels, int length)
{
    for (int i = 0; i + 3 < length; i += 4)
    {
        float r = pixels[i], g = pixels[i + 1], b = pixels[i + 2];
        pixels[i]     = clampByte(r * 0.393f + g * 0.769f + b * 0.189f);
        pixels[i + 1] = clampByte(r * 0.349f + g * 0.686f + b * 0.168f);
        pixels[i + 2] = clampByte(r * 0.272f + g * 0.534f + b * 0.131f);
    }
}

extern "C" EXPORT void Sharpen(unsigned char* pixels, int width, int height)
{
    const int length = width * height * 4;
    std::vector<unsigned char> src(pixels, pixels + length);

    for (int y = 0; y < height; ++y)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int c = 0; c < 3; ++c)
            {
                int idx = (y * width + x) * 4 + c;
                float val = 5.0f * src[idx];
                if (y > 0)           val -= src[((y - 1) * width + x) * 4 + c];
                if (y < height - 1)  val -= src[((y + 1) * width + x) * 4 + c];
                if (x > 0)           val -= src[(y * width + (x - 1)) * 4 + c];
                if (x < width - 1)   val -= src[(y * width + (x + 1)) * 4 + c];
                pixels[idx] = clampByte(val);
            }
        }
    }
}

extern "C" EXPORT void Pixelate(unsigned char* pixels, int width, int height, int blockSize)
{
    if (blockSize < 1) blockSize = 1;

    for (int by = 0; by < height; by += blockSize)
    {
        for (int bx = 0; bx < width; bx += blockSize)
        {
            int sumR = 0, sumG = 0, sumB = 0, count = 0;
            int yEnd = std::min(by + blockSize, height);
            int xEnd = std::min(bx + blockSize, width);

            for (int y = by; y < yEnd; ++y)
                for (int x = bx; x < xEnd; ++x)
                {
                    int idx = (y * width + x) * 4;
                    sumR += pixels[idx];
                    sumG += pixels[idx + 1];
                    sumB += pixels[idx + 2];
                    ++count;
                }

            auto avgR = static_cast<unsigned char>(sumR / count);
            auto avgG = static_cast<unsigned char>(sumG / count);
            auto avgB = static_cast<unsigned char>(sumB / count);

            for (int y = by; y < yEnd; ++y)
                for (int x = bx; x < xEnd; ++x)
                {
                    int idx = (y * width + x) * 4;
                    pixels[idx]     = avgR;
                    pixels[idx + 1] = avgG;
                    pixels[idx + 2] = avgB;
                }
        }
    }
}

extern "C" EXPORT void FlipHorizontal(unsigned char* pixels, int width, int height)
{
    for (int y = 0; y < height; ++y)
        for (int x = 0; x < width / 2; ++x)
        {
            int idxL = (y * width + x) * 4;
            int idxR = (y * width + (width - 1 - x)) * 4;
            for (int c = 0; c < 4; ++c)
                std::swap(pixels[idxL + c], pixels[idxR + c]);
        }
}

extern "C" EXPORT void FlipVertical(unsigned char* pixels, int width, int height)
{
    for (int y = 0; y < height / 2; ++y)
    {
        int rowTop = y * width * 4;
        int rowBot = (height - 1 - y) * width * 4;
        for (int x = 0; x < width * 4; ++x)
            std::swap(pixels[rowTop + x], pixels[rowBot + x]);
    }
}
