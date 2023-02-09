FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2-alpine3.16@sha256:fe6f63ef87eb8a61ad91ef04ae13094ee5bd734ff3bbfb8d43b0d6b1c5f20b3c
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
