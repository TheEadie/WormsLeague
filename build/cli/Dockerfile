FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.5-alpine3.18@sha256:d84acb83e6a166fa6be81c7787c9bb1df2df490adabc54ac8b6e7577aed3c161
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
