#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.304@sha256:f57307946b712ecf86561b3a202053cca07ea0004b782bd8223e213b67b517cd AS build
WORKDIR /app

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.ReplayUpdater.Queues/Worms.Hub.ReplayUpdater.Queues.csproj ./src/Worms.Hub.ReplayUpdater.Queues/Worms.Hub.ReplayUpdater.Queues.csproj
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Hub.ReplayUpdater/Worms.Hub.ReplayUpdater.csproj ./src/Worms.Hub.ReplayUpdater/Worms.Hub.ReplayUpdater.csproj
RUN dotnet restore src/Worms.Hub.ReplayUpdater/Worms.Hub.ReplayUpdater.csproj -r linux-x64

COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.ReplayUpdater.Queues ./src/Worms.Hub.ReplayUpdater.Queues
COPY src/Worms.Hub.Storage ./src/Worms.Hub.Storage
COPY src/Worms.Hub.ReplayUpdater ./src/Worms.Hub.ReplayUpdater
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.ReplayUpdater/Worms.Hub.ReplayUpdater.csproj \
    -c Release \
    -o out \
    --no-restore \
    -r linux-x64 \
    --self-contained true \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM mcr.microsoft.com/dotnet/runtime:9.0.8@sha256:7ce005c8cc7f2f296f6e25131144092c2b781fb4e5809e3651c23e93b68278fd
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.ReplayUpdater"]
