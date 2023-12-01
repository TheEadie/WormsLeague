FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.0-alpine3.18@sha256:d7eb20c03fe6d013801d6f166ecc27725df7455b096660ac32cf0880f1010d44
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
