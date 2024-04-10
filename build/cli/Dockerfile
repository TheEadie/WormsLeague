FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.4-alpine3.18@sha256:4be015e5751b7057aac72a2f5742b162bceb74b2afc262f64d10ca048c42d823
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
