FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.1-alpine3.18@sha256:ddf059bfc910d52288102cc50315e97e6cd065b1e2fe2056e0ecf8f4c678d7ee
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
