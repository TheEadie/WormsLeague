FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.3-alpine3.16@sha256:b80940a662df21ba5b0e9d646bc38f6997f7bb850c70027dd3c09088ae4ac846
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
