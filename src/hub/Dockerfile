#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:6.0.402@sha256:e86848d2834af9afb04eb9f822344178b5aad3b1909a3325be71fb3e98c46be7 AS build
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
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.10-alpine3.16@sha256:74122689b26f2e7717634194a7ecdedda4c7a39dd5c96865206513010b2a4379
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Gateway"]
