FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.1-alpine3.18@sha256:c9d63f33175836708e81fb1fa1e4f402ebbd5375cbe3e73613e566257dfe3571
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
