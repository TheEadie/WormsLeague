FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.0-alpine3.20@sha256:273df0607bac8879a1ae5fa1a2f9d80246fef56aefe8928bd470aedb2479e89e
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
