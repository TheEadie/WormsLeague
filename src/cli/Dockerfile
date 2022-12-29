FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.1-alpine3.16@sha256:bc63d574233d2ae2c734f4359c2e08482e132dba0d7cbba2401cc641ca2e7d84
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
