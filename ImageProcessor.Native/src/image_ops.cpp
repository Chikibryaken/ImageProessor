#include "image_ops.h"
#include <vector>

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
