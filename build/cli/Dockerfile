FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.0-alpine3.18@sha256:f40ba3676c3dbbe013e03a765aab1c2dad7b72979412961dc81f086461759ce0
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
