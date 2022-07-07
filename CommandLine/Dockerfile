FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.6-alpine3.15@sha256:1a0862535feb9d53683a0c2a91b090df62603a5b50c32a30b17e5027884ad145
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]