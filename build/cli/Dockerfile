FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.6-alpine3.18@sha256:8ff1a6da9931f9c3ad42d0873d7d8eb4c56d67727ae9bf23cabe9ebdc256eb4c
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
