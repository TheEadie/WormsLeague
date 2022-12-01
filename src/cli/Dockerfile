FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.0-alpine3.16@sha256:35a61da030416bcd001bce374bbd243c67b9986c47ce5f12c6db7c0c704f50a8
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
