FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.6-alpine3.15@sha256:04229238e8d4c7fd1a00ff93c0fae695da67e18e072446cef5a8c515ff250434
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]