#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.101@sha256:7ef41132b2ebe6166bde36b7ba2f0d302e10307c3e0523a4539643a77233f56d AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0.1@sha256:3ff67792728179308c4bf06799d8b45d155c60fddc347acf69465a496d9a20b8
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
