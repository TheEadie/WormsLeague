FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.10-alpine3.16@sha256:0220063e51b940f8a74605103d083a034c6a81b8b2cd272fc9a9e1c2cc616311
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
