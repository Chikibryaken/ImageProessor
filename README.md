# ImageProcessor

Веб-сервис обработки изображений с нативным C++ ядром.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![C++17](https://img.shields.io/badge/C++-17-00599C?logo=cplusplus)
![Docker|114](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker)
![GitHub Pages](https://img.shields.io/badge/GitHub_Pages-deployed-222222?logo=github)

## Architecture

```
[Browser]
    │
    ▼
[Blazor WASM]          ← GitHub Pages (static hosting)
    │  HTTP/HTTPS
    ▼
[ASP.NET Core API]     ← Docker / Railway
    │  P/Invoke
    ▼
[C++ .so / .dll]       ← компилируется внутри Docker (g++) или MSVC (VS)
```

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10) |
| API | ASP.NET Core Minimal API (.NET 10) |
| Image codec | SixLabors.ImageSharp |
| Native core | C++17 (.dll на Windows, .so на Linux) |
| Interop | P/Invoke (`[DllImport]`) |
| Containerization | Docker + docker-compose |
| CI/CD | GitHub Actions |

## Why C++?

Пиксельные операции (invert, grayscale, box blur) работают над миллионами байт.
Managed C# добавляет overhead JIT и bounds-checking на каждое обращение к массиву.
C++ с `-O2` компилируется в прямые SIMD-инструкции процессора — та же логика
выполняется быстрее без смены архитектуры. Плюс: демонстрация P/Invoke interop
как паттерна для высоконагруженных библиотек (ffmpeg, OpenCV, LAPACK).

## Run locally

### Visual Studio (Windows)

1. Открой `ImageProessor.sln`
2. Собери solution — C++ DLL соберётся автоматически
3. Запусти `ImageProcessor.Api` через **Ctrl+F5** (без отладчика)
4. Запусти `ImageProcessor.Web` через **Ctrl+F5** в отдельном окне
5. Swagger API: `https://localhost:60757/swagger`

### Docker

```bash
docker-compose up --build
```

API будет доступен на `http://localhost:8080`.

## Architecture decisions

### Почему P/Invoke, а не отдельный микросервис для C++?

Микросервис добавил бы сетевой вызов (latency), сериализацию изображений (overhead),
отдельный деплой и мониторинг. P/Invoke — прямой вызов функции в том же процессе:
нет копирования данных по сети, нет маршалинга JSON/protobuf. Для CPU-bound операций
над бинарными данными это правильный выбор.

### Почему Minimal API, а не Controllers?

Три эндпоинта не требуют MVC-инфраструктуры: фильтров, биндеров моделей,
area-роутинга. Minimal API — это чистый код: регистрация эндпоинта = одна лямбда.
Меньше слоёв абстракции → проще читать, проще тестировать через `WebApplicationFactory`.

### Почему Singleton для ImageProcessingService?

`ImageProcessingService` не хранит состояние между запросами (все данные — параметры
методов). Singleton избегает аллокации нового экземпляра на каждый запрос.
`NativeImageProcessor` внутри него — тонкая обёртка над P/Invoke без полей: тоже
безопасен как Singleton. ImageSharp создаётся внутри каждого вызова `ProcessAsync`.

## Screenshot

![UI Screenshot](docs/screenshot1.png)
![UI Screenshot](docs/screenshot2.png)