FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.4-alpine3.16@sha256:d05253254e0923267bb900607e5adddaa7131343b62d3951c81f8a34f0c1bcc9
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
