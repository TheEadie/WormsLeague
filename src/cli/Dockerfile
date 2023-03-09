FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.3-alpine3.16@sha256:7bf13d0c0818591f2bc49995710199f0033879ab2817b243d6d57d8002807117
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
