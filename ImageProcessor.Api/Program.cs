using Microsoft.AspNetCore.Mvc;
using ImageProcessor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ImageProcessor API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddSingleton<INativeImageProcessor, NativeImageProcessor>();
builder.Services.AddSingleton<ImageProcessingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/images/test-native", (ImageProcessingService svc) =>
    Results.Ok(svc.TestNativeCall()))
.WithTags("Diagnostics");

app.MapPost("/api/images/process", async (
    [FromForm] ProcessRequest request,
    ImageProcessingService svc) =>
{
    if (request.File is null || request.File.Length == 0)
        return Results.BadRequest("Файл не загружен или пустой.");

    if (string.IsNullOrWhiteSpace(request.Filter))
        return Results.BadRequest("Фильтр не указан.");

    try
    {
        await using var stream = request.File.OpenReadStream();
        float intensity = request.Intensity ?? 1.0f;
        int blurIterations = Math.Clamp(request.BlurStrength ?? 5, 1, 20);
        byte[] resultPng = await svc.ProcessAsync(stream, request.Filter, intensity, blurIterations);
        return Results.File(resultPng, contentType: "image/png", fileDownloadName: "processed.png");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Ошибка обработки изображения",
            detail: ex.Message,
            statusCode: 500);
    }
})
.DisableAntiforgery()
.WithName("ProcessImage")
.WithTags("Images")
.Produces(400);

app.Run();

public partial class Program { }

public class ProcessRequest
{
    public IFormFile? File { get; set; }
    public string? Filter { get; set; }
    public int? BlurStrength { get; set; }
    public float? Intensity { get; set; }
}
