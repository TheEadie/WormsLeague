#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:6.0.301@sha256:09c76ca233d3a413fd72bd0bc8c342ed12a8b55f1cc29ac455c049db93632712 AS build
WORKDIR /app

COPY *.sln ./
COPY src/Worms.Gateway/Worms.Gateway.csproj ./src/Worms.Gateway/
RUN dotnet restore --runtime alpine-x64

COPY .editorconfig ./src/
COPY src/ ./src/
ARG VERSION=0.0.1
RUN dotnet publish \
        "src/Worms.Gateway/Worms.Gateway.csproj" \
        -c Release \
        --runtime alpine-x64 \
        --self-contained \
        -p:PublishTrimmed=true \
        -o out \
        --no-restore \
        -p:AssemblyVersion=${VERSION} \
        -p:Version=${VERSION}

#### Runtime ####
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.6-alpine3.15@sha256:04229238e8d4c7fd1a00ff93c0fae695da67e18e072446cef5a8c515ff250434
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Gateway"]