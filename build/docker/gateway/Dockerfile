#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.201@sha256:4b684e6c74ab8dff26ac54c79d8242b1dd05aba06c367de2b583bad79fd6399b AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:8.0.2@sha256:789045ecae51d62d07877994d567eff4442b7bbd4121867898ee7bf00b7241ea
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Gateway"]
