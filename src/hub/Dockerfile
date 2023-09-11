#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:7.0.400@sha256:bdcfb498261ca18f023ac67615d814ea743aa3288eb880855fa2eb86c6313ccc AS build
WORKDIR /app
ARG PROJECT="src/Worms.Gateway/Worms.Gateway.csproj"

COPY *.sln ./
COPY src/Worms.Gateway/Worms.Gateway.csproj ./src/Worms.Gateway/
RUN dotnet restore ${PROJECT} --runtime alpine-x64

COPY src/ ./src/
ARG VERSION=0.0.1
RUN dotnet publish \
    ${PROJECT} \
    -c Release \
    --runtime alpine-x64 \
    --self-contained \
    -p:PublishTrimmed=true \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Runtime ####
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4-alpine3.16@sha256:d05253254e0923267bb900607e5adddaa7131343b62d3951c81f8a34f0c1bcc9
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Gateway"]
