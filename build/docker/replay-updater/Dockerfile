#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.306@sha256:a5dd7352c0c058a6f847c95a1147b060e95a532444f14b34d8fa9aaa0a76702f AS build
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
FROM mcr.microsoft.com/dotnet/runtime:9.0.10@sha256:cca0696675740e83a8241400ea75a0a938fa8b547f0908cbebb30f2f7524101e
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.ReplayUpdater"]
