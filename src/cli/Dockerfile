FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.1-alpine3.16@sha256:95166e1ee24f8006d0088f29e873c6f08d7d27b42b10c067bd7ebd12707260c8
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
