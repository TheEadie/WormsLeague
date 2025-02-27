FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.2-alpine3.20@sha256:7b863ba0b146a987977562afcd4e23577fe9fa0128f8f9b07575618cc38a8194
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
