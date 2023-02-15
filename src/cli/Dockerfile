FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.3-alpine3.16@sha256:5f5844b325fb93f76534fc0176105d4b3956f7eedf4bcc4d145f24db91d938eb
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
