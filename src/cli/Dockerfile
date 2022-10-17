FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.10-alpine3.16@sha256:74122689b26f2e7717634194a7ecdedda4c7a39dd5c96865206513010b2a4379
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
