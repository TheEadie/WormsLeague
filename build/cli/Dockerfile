FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.0-alpine3.20@sha256:1478697b27d89ce203e905d72861a1f3f70c22d1abbb50f96a594d0ebbb8dbf6
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
