#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:8.0.303@sha256:7d0ba26469267b563120456557e38eccef9972cb6b9cfbbd47a50d1218fa7b30 AS build
WORKDIR /app

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj ./src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj
COPY src/Worms.Hub.Storage/Worms.Hub.Storage.csproj ./src/Worms.Hub.Storage/Worms.Hub.Storage.csproj
COPY src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj ./src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj
RUN dotnet restore src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj

COPY src/Worms.Armageddon.Game ./src/Worms.Armageddon.Game
COPY src/Worms.Hub.Storage ./src/Worms.Hub.Storage
COPY src/Worms.Hub.ReplayProcessor ./src/Worms.Hub.ReplayProcessor
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj \
    -c Release \
    -o out \
    --no-restore \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM ubuntu:22.04@sha256:340d9b015b194dc6e2a13938944e0d016e57b9679963fdeb9ce021daac430221 as runtime

RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  ca-certificates \
  wget \
  gnupg \
  dotnet-runtime-8.0 \
  && rm -rf /var/lib/apt/lists/*

# Get key to Wine repo
RUN wget -nc https://dl.winehq.org/wine-builds/winehq.key -O /tmp/winehq.key

# Add Wine repo
RUN apt-get update \
  && apt-get install -y software-properties-common \
  && apt-key add /tmp/winehq.key \
  && add-apt-repository 'deb https://dl.winehq.org/wine-builds/ubuntu/ jammy main' \
  && rm /tmp/winehq.key \
  && rm -rf /var/lib/apt/lists/*

# AMD 32-bit deps for Wine
RUN dpkg --add-architecture i386 \
  && apt-get update \
  && apt-get install -y --install-recommends winehq-stable wine32 xvfb \
  && rm -rf /var/lib/apt/lists/*

# Create WINEPREFIX
RUN WINEDLLOVERRIDES="mscoree,mshtml=" xvfb-run wineboot -i \
  && wineserver -k

# Copy game installation directory

# Some settings for WA
RUN wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v WineCompatibilitySuggested /d 0x7FFFFFFF /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v WindowedMode /d 0x00000001 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v DetailLevel /d 0x00000005 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v WindowedMode /d 0x00000001 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v PinnedChatLines /d 0x00000007 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v InfoTransparency /d 0x00000001 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v InfoSpy /d 0x00000001 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v DisableSmoothBackgroundGradient /d 0x00000000 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v HardwareRendering /d 0x00000001 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v LargerFonts /d 0x00000000 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v AssistedVsync /d 0x00000000 /f \
  && wine reg add 'HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options' /t REG_DWORD /v Vsync /d 0x00000000 /f \
  && wineserver -k

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.ReplayProcessor"]
