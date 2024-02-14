FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.2-alpine3.18@sha256:cd9800407641e76cb327cb540d5dda54fc4c9bf96a69aeceddd3f40771f4e940
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
