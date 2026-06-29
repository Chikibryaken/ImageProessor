using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace ImageProcessor.Tests.Integration;

public class ImageProcessingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ImageProcessingApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static ByteArrayContent PngContent()
    {
        using var image = new Image<Rgba32>(4, 4, new Rgba32(255, 0, 0, 255));
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        var content = new ByteArrayContent(ms.ToArray());
        content.Headers.ContentType = new("image/png");
        return content;
    }

    [Fact]
    public async Task Process_ValidPngWithInvertFilter_Returns200AndPngBody()
    {
        using var form = new MultipartFormDataContent();
        form.Add(PngContent(), "file", "test.png");
        form.Add(new StringContent("invert"), "filter");

        var response = await _client.PostAsync("/api/images/process", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(body);
    }

    [Fact]
    public async Task Process_NoFile_Returns400()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("invert"), "filter");

        var response = await _client.PostAsync("/api/images/process", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Process_UnknownFilter_Returns400()
    {
        using var form = new MultipartFormDataContent();
        form.Add(PngContent(), "file", "test.png");
        form.Add(new StringContent("unknown"), "filter");

        var response = await _client.PostAsync("/api/images/process", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
