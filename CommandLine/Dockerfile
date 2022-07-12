FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.7-alpine3.15@sha256:f8f99a3ed1492367ebe1902ae3f9ce7f755bd2a3c0685ce0abb4e291dd08792a
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]