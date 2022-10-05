#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:6.0.401@sha256:fc57fca25ca6b7de2ffc260c145a27eec47f692211c117456f09f15d34f3a9a2 AS build
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
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.9-alpine3.16@sha256:a4cc5aa9352d4a0fc2583f912bc593fd008027aedbd54f71ea27ce081812ea80
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Gateway"]
