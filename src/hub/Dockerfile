#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:6.0.401@sha256:2fdad630a59fbdecbfcc6aaf3ccae96eea0e8d24cd4607669fde0de107a94c53 AS build
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
