FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.8-alpine3.15@sha256:f5d816de85b96d03a0eeafbeac62339550bcabb12fa920768eac4417182bee4a
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]