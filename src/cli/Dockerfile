FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.2-alpine3.16@sha256:6249eaab9d07b62ae80092fe46b81ad5dbe7a6ea23ee0ead9bfbe1048b6f1d51
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
