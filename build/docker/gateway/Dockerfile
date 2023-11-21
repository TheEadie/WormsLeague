#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.100@sha256:239eb99884b49bac13f4ff5935e118654f87b67d6e31bc842c7cd5a6c46fcc4f AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0@sha256:09539bb9ccc51d3a2b3c205101001e665bbbe5e62c94d6377bb0f71b24ea20cb
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
