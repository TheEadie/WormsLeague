#### Web Build ####
FROM node:24-alpine@sha256:d32cdf619f63fe0471182d08996dd516c6275bb5fd31ae06e55a570bd9e1ad43 AS node-build
WORKDIR /repo

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json src/Worms.Hub.Web/
RUN cd src/Worms.Hub.Web && npm ci

COPY src/Worms.Hub.Web/. src/Worms.Hub.Web/
RUN cd src/Worms.Hub.Web && VITE_GATEWAY_URL= npx vite build

#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:10.0.400@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c AS build
WORKDIR /app

COPY Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj ./src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj
COPY src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj ./src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj
RUN dotnet restore src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj

COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.Storage ./src/Worms.Hub.Storage
COPY src/Worms.Armageddon.Files ./src/Worms.Armageddon.Files
COPY src/Worms.Hub.Gateway ./src/Worms.Hub.Gateway
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.Gateway/Worms.Hub.Gateway.csproj \
    -c Release \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

COPY --from=node-build /repo/.artifacts/web/ ./out/wwwroot/

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM mcr.microsoft.com/dotnet/aspnet:10.0.11@sha256:a4556ed033fa96f984bb7a8d348851cb2d36b1281dd2420070045f664fbb5f94
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
