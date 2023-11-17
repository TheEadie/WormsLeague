FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.0-alpine3.18@sha256:4e9ae5dabdf4177c83f572fb5d500e92aa4c000469806292af8e3cff36d0f472
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
