using System.Runtime.InteropServices;

// ──────────────────────────────────────────────────────────────────────────────
// Тестовые данные: два пикселя в RGBA
//   Пиксель 1: ярко-красный  (255, 0, 0, 255)  → должен стать (0, 255, 255, 255)
//   Пиксель 2: полупрозрачный зелёный (0, 200, 0, 128) → (255, 55, 255, 128)
// ──────────────────────────────────────────────────────────────────────────────
byte[] pixels =
[
    255,   0,   0, 255,   // пиксель 1
      0, 200,   0, 128,   // пиксель 2
];

Console.WriteLine("=== P/Invoke smoke test: InvertColors ===");
Console.WriteLine();
Console.WriteLine("До инверсии:");
PrintPixels(pixels);

NativeMethods.InvertColors(pixels, pixels.Length);

Console.WriteLine("После инверсии:");
PrintPixels(pixels);

Console.WriteLine();
Console.WriteLine("Ожидается:");
Console.WriteLine("  Pixel 0: R=  0  G=255  B=255  A=255");
Console.WriteLine("  Pixel 1: R=255  G= 55  B=255  A=128");

static void PrintPixels(byte[] data)
{
    int pixelCount = data.Length / 4;
    for (int i = 0; i < pixelCount; i++)
    {
        int o = i * 4;
        Console.WriteLine($"  Pixel {i}: R={data[o],3}  G={data[o + 1],3}  B={data[o + 2],3}  A={data[o + 3],3}");
    }
    Console.WriteLine();
}

// ──────────────────────────────────────────────────────────────────────────────
// P/Invoke — механизм, которым CLR вызывает нативные функции из DLL.
// Объявления типов (class, struct и т.д.) должны идти ПОСЛЕ top-level statements.
// ──────────────────────────────────────────────────────────────────────────────
static class NativeMethods
{
    [DllImport("ImageProcessor.Native",
               EntryPoint = "InvertColors",
               CallingConvention = CallingConvention.Cdecl)]
    public static extern void InvertColors([In, Out] byte[] pixels, int length);
}