#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.204@sha256:587d86d535f6bd584623f6643dee3b4b5176a40ec750eb33ff524d3112d19a91 AS build
WORKDIR /app
ARG PROJECT="Worms.Hub.Gateway.csproj"

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Hub.Gateway/${PROJECT} ./${PROJECT}
RUN dotnet restore ${PROJECT}

COPY src/Worms.Hub.Gateway .
ARG VERSION=0.0.1
RUN dotnet publish \
    ${PROJECT} \
    -c Release \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Test ####
FROM build as test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM mcr.microsoft.com/dotnet/aspnet:8.0.4@sha256:171211e17eb819d7b4d404933d8b9cf6d7a7d58d4dcbe50b4259bfa4a72a7ed3
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
