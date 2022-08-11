FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.8-alpine3.15@sha256:271045cc1f6ab63a16df35a77fd1863d6c34d7fd85d9681856769a2522b7d70d
WORKDIR /app
COPY ./.artifacts/linux-musl-x64/worms ./
RUN chmod +x ./worms
ENTRYPOINT ["./worms"]