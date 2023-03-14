FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4-alpine3.16@sha256:b4c4c9c042f1d44b7292e22f4f6e3f3a878a9c3bfcdfae83162283a5a71341a5
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
