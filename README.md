# ImageProcessor

A web image processing service with a C++ native core, exposed through ASP.NET Core and consumed by a Blazor WebAssembly frontend.

## Live Demo

- **App:** https://chikibryaken.github.io/ImageProessor/
- **API:** https://imageproessor-production.up.railway.app/swagger

## Features

Ten real-time filters applied server-side via C++ pixel operations:

| Filter | Description |
|---|---|
| Invert | Inverts each RGB channel |
| Grayscale | BT.601 luminance conversion |
| Sepia | Classic sepia tone matrix |
| Blur | Iterative 3×3 box blur |
| Sharpen | 5-tap convolution kernel |
| Brightness | Multiply RGB by a factor (0.1–3.0) |
| Contrast | Stretch values around midpoint 128 |
| Pixelate | Block-average downsampling |
| Flip Horizontal | Mirror along the vertical axis |
| Flip Vertical | Mirror along the horizontal axis |

## Architecture

```
[Browser]
    │
    ▼
[Blazor WASM] ──────────────────────── GitHub Pages
    │  HTTP multipart/form-data
    ▼
[ASP.NET Core Minimal API] ─────────── Docker / Railway
    │  P/Invoke
    ▼
[C++ .so / .dll] ───────────────────── compiled by g++ inside Docker
```

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10) |
| Backend | ASP.NET Core Minimal API (.NET 10) |
| Native | C++17 (GCC on Linux / MSVC on Windows) |
| Bridge | P/Invoke (`[DllImport]`) |
| Image codec | SixLabors.ImageSharp |
| Container | Docker (Linux, multi-stage build) |
| CI/CD | GitHub Actions |
| Hosting | Railway (API) + GitHub Pages (frontend) |

## Why C++?

Pixel operations iterate over millions of bytes per image. C++ compiled with `-O2` produces direct machine instructions with no GC pressure or JIT overhead. The project also demonstrates end-to-end native interop: writing platform-native code, exporting it with `extern "C"`, and consuming it from managed .NET via P/Invoke — a pattern used in production for FFmpeg, OpenCV, and LAPACK bindings.

## Run Locally

### Prerequisites

- Visual Studio 2022 or later
- .NET 10 SDK
- Docker Desktop (optional)

### Steps

1. Clone the repository
2. Open `ImageProessor.sln` in Visual Studio
3. Set multiple startup projects: `ImageProcessor.Api` + `ImageProcessor.Web`
4. Press **Ctrl+F5**

The API runs on `https://localhost:60757`, the frontend on `https://localhost:5001`.

### Docker

```bash
docker-compose up --build
```

The API is available at `http://localhost:8080`. The Blazor frontend is deployed separately to GitHub Pages and is not included in the Docker image.

## Architecture Decisions

**Why P/Invoke instead of a separate microservice?**
P/Invoke is a direct in-process function call — zero network latency, no serialization, no extra deployment unit. For CPU-bound binary operations this is the right boundary.

**Why Minimal API instead of Controllers?**
Three endpoints don't justify MVC infrastructure. A Minimal API endpoint is a single lambda — easier to read, easier to test via `WebApplicationFactory`, less indirection.

**Why Singleton for `ImageProcessingService`?**
The service holds no per-request state. Singleton avoids allocation on every request. `NativeImageProcessor` wraps stateless P/Invoke calls and is equally safe as a singleton.
