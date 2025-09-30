#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.305@sha256:123b43e4d9775451d8ed63af324a42132707b4edb14770bcdf5c85cf55bc45f1 AS build
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
FROM mcr.microsoft.com/dotnet/runtime:9.0.9@sha256:6e6ccf450a40a99748e93bf18dd6739893881f5dd831ae23b586565749b1643d
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.ReplayUpdater"]
