FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.9-alpine3.16@sha256:a4cc5aa9352d4a0fc2583f912bc593fd008027aedbd54f71ea27ce081812ea80
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
