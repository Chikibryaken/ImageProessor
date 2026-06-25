# ── Stage 1: компилируем C++ ядро в .so ─────────────────────────────────────
# Используем SDK-образ: в нём есть apt и g++ доступен через пакетный менеджер.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-native

RUN apt-get update \
    && apt-get install -y --no-install-recommends g++ \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ImageProcessor.Native/ ImageProcessor.Native/

# Флаги g++:
#   -shared   создать разделяемую библиотеку (.so) вместо исполняемого файла
#   -fPIC     Position-Independent Code: адреса инструкций относительны,
#             что обязательно для .so — библиотека загружается в произвольное
#             место адресного пространства каждого процесса
#   -O2       оптимизация (скорость без заметного раздутия бинарника)
#   -std=c++17 нужен для std::vector в нашем коде
#   -I        путь к заголовочным файлам (image_ops.h лежит в include/)
RUN g++ -shared -fPIC -O2 -std=c++17 \
        -I ImageProcessor.Native/include \
        ImageProcessor.Native/src/image_ops.cpp \
        -o libImageProcessor.Native.so


# ── Stage 2: собираем ASP.NET Core API ───────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-dotnet

WORKDIR /src
# Копируем .csproj отдельным слоем: если изменился только C#-код,
# Docker переиспользует кэш restore и не скачивает пакеты заново.
COPY ImageProcessor.Api/ImageProcessor.Api.csproj ImageProcessor.Api/
RUN dotnet restore ImageProcessor.Api/ImageProcessor.Api.csproj

COPY ImageProcessor.Api/ ImageProcessor.Api/
RUN dotnet publish ImageProcessor.Api/ImageProcessor.Api.csproj \
    -c Release -o /app/publish


# ── Stage 3: минимальный runtime-образ ───────────────────────────────────────
# aspnet — только ASP.NET Core runtime без SDK (≈200 МБ против ≈800 МБ у sdk)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build-dotnet /app/publish .

# Копируем .so рядом с ImageProcessor.Api.dll.
# CLR на Linux при [DllImport("ImageProcessor.Native")] ищет файл
# libImageProcessor.Native.so в директории процесса и в путях ld.so.
COPY --from=build-native /src/libImageProcessor.Native.so .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ImageProcessor.Api.dll"]
