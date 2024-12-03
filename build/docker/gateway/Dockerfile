#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.101@sha256:fe8ceeca5ee197deba95419e3b85c32744970b730ae11645e13f1cb74a848d98 AS build
WORKDIR /app

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj ./src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj
RUN dotnet restore src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj

COPY src/Worms.Hub.Storage ./src/Worms.Hub.Storage
COPY src/Worms.Hub.Gateway ./src/Worms.Hub.Gateway
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj \
    -c Release \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0@sha256:d8f01f752bf9bd3ff630319181a2ccfbeecea4080a1912095a34002f61bfa345
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
