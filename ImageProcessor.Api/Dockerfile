FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-native

RUN apt-get update \
    && apt-get install -y --no-install-recommends g++ \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ImageProcessor.Native/ ImageProcessor.Native/

RUN g++ -shared -fPIC -O2 -std=c++17 \
        -I ImageProcessor.Native/include \
        ImageProcessor.Native/src/image_ops.cpp \
        -o libImageProcessor.Native.so


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-dotnet

WORKDIR /src
COPY ImageProcessor.Api/ImageProcessor.Api.csproj ImageProcessor.Api/
RUN dotnet restore ImageProcessor.Api/ImageProcessor.Api.csproj

COPY ImageProcessor.Api/ ImageProcessor.Api/
RUN dotnet publish ImageProcessor.Api/ImageProcessor.Api.csproj \
    -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build-dotnet /app/publish .
COPY --from=build-native /src/libImageProcessor.Native.so .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ImageProcessor.Api.dll"]
