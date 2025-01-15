FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.1-alpine3.20@sha256:beee9ae3599b372ad6d1f860d690d22f53180da1f0c3afae7875c3ee9acf51a7
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
