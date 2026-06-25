namespace ImageProcessor.Api.Services;

public interface INativeImageProcessor
{
    void InvertColors(byte[] pixels, int length);
    void Grayscale(byte[] pixels, int length);
    void Blur(byte[] pixels, int width, int height, int iterations);
}
