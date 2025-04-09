FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.4-alpine3.20@sha256:faa24fd9207bf88b1e84f78c26d1a5a02feacd453a178b2451e3e079f73fdf9a
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
