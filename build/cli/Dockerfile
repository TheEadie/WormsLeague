FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.5-alpine3.18@sha256:d1c8f51e5937fccb40d0e12a743614ecde2e8beb068edac5ccce493c3757eb18
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
