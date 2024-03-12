FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.3-alpine3.18@sha256:e13d100b0c511b0b998a4706191495d6b1ec3b05a5257e0a0c956bce00c3daa4
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
