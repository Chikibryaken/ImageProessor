namespace ImageProcessor.Api.Services;

public interface INativeImageProcessor
{
    void InvertColors(byte[] pixels, int length);
    void Grayscale(byte[] pixels, int length);
    void Blur(byte[] pixels, int width, int height, int iterations);
    void Brightness(byte[] pixels, int length, float factor);
    void Contrast(byte[] pixels, int length, float factor);
    void Sepia(byte[] pixels, int length);
    void Sharpen(byte[] pixels, int width, int height);
    void Pixelate(byte[] pixels, int width, int height, int blockSize);
    void FlipHorizontal(byte[] pixels, int width, int height);
    void FlipVertical(byte[] pixels, int width, int height);
}
