#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:9.0.305@sha256:123b43e4d9775451d8ed63af324a42132707b4edb14770bcdf5c85cf55bc45f1 AS build
WORKDIR /app

COPY src/Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj ./src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.ReplayProcessor.Queues/Worms.Hub.ReplayProcessor.Queues.csproj ./src/Worms.Hub.ReplayProcessor.Queues/Worms.Hub.ReplayProcessor.Queues.csproj
COPY src/Worms.Hub.ReplayUpdater.Queues/Worms.Hub.ReplayUpdater.Queues.csproj ./src/Worms.Hub.ReplayUpdater.Queues/Worms.Hub.ReplayUpdater.Queues.csproj
COPY src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj ./src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj
RUN dotnet restore src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj -r linux-x64

COPY src/Worms.Armageddon.Game ./src/Worms.Armageddon.Game
COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.ReplayProcessor.Queues ./src/Worms.Hub.ReplayProcessor.Queues
COPY src/Worms.Hub.ReplayUpdater.Queues ./src/Worms.Hub.ReplayUpdater.Queues
COPY src/Worms.Hub.ReplayProcessor ./src/Worms.Hub.ReplayProcessor
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.ReplayProcessor/Worms.Hub.ReplayProcessor.csproj \
    -c Release \
    -o out \
    --no-restore \
    -r linux-x64 \
    --self-contained true \
    -p:AssemblyVersion=${VERSION} \
    -p:Version=${VERSION}

#### Test ####
FROM build AS test
RUN dotnet test --no-restore --no-build --verbosity normal

#### Runtime ####
FROM ubuntu:22.04@sha256:1aa979d85661c488ce030ac292876cf6ed04535d3a237e49f61542d8e5de5ae0 as runtime

RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  ca-certificates \
  wget \
  gnupg \
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
