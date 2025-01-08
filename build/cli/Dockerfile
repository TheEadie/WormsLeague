FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.0-alpine3.20@sha256:af88eef3a293a3d57b49f1e9114f6a6b17f7525c651ef257262c10ab5b8cd1bf
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
