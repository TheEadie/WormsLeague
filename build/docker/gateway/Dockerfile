#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.402@sha256:df188b6cb642450d83d3d857a992bbbef248212fd456dcb9ae582ddd1bb96dce AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0.8@sha256:30d8619d9a4f68508d9b17fc2088e857e629d3f9ceaaf57c22d6747f7326d89e
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
