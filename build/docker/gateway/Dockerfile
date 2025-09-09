#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.304@sha256:f57307946b712ecf86561b3a202053cca07ea0004b782bd8223e213b67b517cd AS build
WORKDIR /app

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.ReplayProcessor.Queues/Worms.Hub.ReplayProcessor.Queues.csproj ./src/Worms.Hub.ReplayProcessor.Queues/Worms.Hub.ReplayProcessor.Queues.csproj
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj ./src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj
RUN dotnet restore src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj

COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.ReplayProcessor.Queues ./src/Worms.Hub.ReplayProcessor.Queues
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
FROM mcr.microsoft.com/dotnet/aspnet:9.0.8@sha256:49ae5b354037606b2339b723ff9a3be7827eb53e513dfd683cf39a78cabb6a2b
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
