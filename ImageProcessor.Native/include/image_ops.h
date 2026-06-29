#pragma once

#ifdef _WIN32
    #ifdef IMAGEPROCESSORNATIVE_EXPORTS
        #define EXPORT __declspec(dllexport)
    #else
        #define EXPORT __declspec(dllimport)
    #endif
#else
    #define EXPORT __attribute__((visibility("default")))
#endif

extern "C"
{
    EXPORT void InvertColors(unsigned char* pixels, int length);
    EXPORT void Grayscale(unsigned char* pixels, int length);
    EXPORT void Blur(unsigned char* pixels, int width, int height, int iterations);
    EXPORT void Brightness(unsigned char* pixels, int length, float factor);
    EXPORT void Contrast(unsigned char* pixels, int length, float factor);
    EXPORT void Sepia(unsigned char* pixels, int length);
    EXPORT void Sharpen(unsigned char* pixels, int width, int height);
    EXPORT void Pixelate(unsigned char* pixels, int width, int height, int blockSize);
    EXPORT void FlipHorizontal(unsigned char* pixels, int width, int height);
    EXPORT void FlipVertical(unsigned char* pixels, int width, int height);
}
