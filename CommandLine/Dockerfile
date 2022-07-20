FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.7-alpine3.15@sha256:116168e8bbc89d252fff7d5e0bfe9f84f8b5ecaf2a4656ca702c5678f564a19a
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]