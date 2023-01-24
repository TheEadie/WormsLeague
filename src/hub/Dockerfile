#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:7.0.102@sha256:028d360c0f409f13daf8689edfe680dba9d4b651eb8a6f1210a3542f0ad4bf58 AS build
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
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2-alpine3.16@sha256:6249eaab9d07b62ae80092fe46b81ad5dbe7a6ea23ee0ead9bfbe1048b6f1d51
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Gateway"]
