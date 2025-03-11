FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.3-alpine3.20@sha256:f9f15be85c20f0ca83de1e2bcefdb758e964e7cbbf7900d29cc9c3ac23c9c328
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]
