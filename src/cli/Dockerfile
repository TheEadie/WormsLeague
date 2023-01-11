FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2-alpine3.16@sha256:9b891ee85af2eb63f68431f0f432d8cafb5b77698ba2fa7136f3109af33820b0
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
