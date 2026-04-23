#### Build ####
FROM mcr.microsoft.com/dotnet/sdk:10.0.203@sha256:8a90a473da5205a16979de99d2fc20975e922c68304f5c79d564e666dc3982fc AS build
WORKDIR /app

COPY Directory.Build.props .
COPY .editorconfig .
COPY src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj ./src/Worms.Armageddon.Files/Worms.Armageddon.Files.csproj
COPY src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj ./src/Worms.Armageddon.Game/Worms.Armageddon.Game.csproj
COPY src/Worms.Armageddon.Gifs/Worms.Armageddon.Gifs.csproj ./src/Worms.Armageddon.Gifs/Worms.Armageddon.Gifs.csproj
COPY src/Worms.Hub.Queues/Worms.Hub.Queues.csproj ./src/Worms.Hub.Queues/Worms.Hub.Queues.csproj
COPY src/Worms.Hub.Armageddon.Runner/Worms.Hub.Armageddon.Runner.csproj ./src/Worms.Hub.Armageddon.Runner/Worms.Hub.Armageddon.Runner.csproj
RUN dotnet restore src/Worms.Hub.Armageddon.Runner/Worms.Hub.Armageddon.Runner.csproj -r linux-x64

COPY src/Worms.Armageddon.Files ./src/Worms.Armageddon.Files
COPY src/Worms.Armageddon.Game ./src/Worms.Armageddon.Game
COPY src/Worms.Armageddon.Gifs ./src/Worms.Armageddon.Gifs
COPY src/Worms.Hub.Queues ./src/Worms.Hub.Queues
COPY src/Worms.Hub.Armageddon.Runner ./src/Worms.Hub.Armageddon.Runner
ARG VERSION=0.0.1
RUN dotnet publish \
    src/Worms.Hub.Armageddon.Runner/Worms.Hub.Armageddon.Runner.csproj \
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
FROM ubuntu:24.04@sha256:c4a8d5503dfb2a3eb8ab5f807da5bc69a85730fb49b5cfca2330194ebcc41c7b AS runtime

# Install Wine, xvfb, configure WINEPREFIX and WA settings in a single layer
# to avoid intermediate files bloating the image
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
    ca-certificates \
    wget \
    gnupg \
    software-properties-common \
  # Add Wine repo
  && wget -nc https://dl.winehq.org/wine-builds/winehq.key -O /tmp/winehq.key \
  && apt-key add /tmp/winehq.key \
  && add-apt-repository 'deb https://dl.winehq.org/wine-builds/ubuntu/ noble main' \
  && rm /tmp/winehq.key \
  # Install Wine with 32-bit support
  && dpkg --add-architecture i386 \
  && apt-get update \
  && apt-get install -y --no-install-recommends winehq-stable wine32 xvfb \
  # Remove packages only needed for setup
  && apt-get purge -y wget gnupg software-properties-common \
  && apt-get autoremove -y \
  # Create WINEPREFIX
  && WINEDLLOVERRIDES="mscoree,mshtml=" xvfb-run wineboot -i \
  && wineserver -k \
  # Configure WA settings
  && xvfb-run sh -c '\
    wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v WineCompatibilitySuggested /d 0x7FFFFFFF /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v WindowedMode /d 0x00000001 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v DetailLevel /d 0x00000005 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v PinnedChatLines /d 0x00000007 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v InfoTransparency /d 0x00000001 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v InfoSpy /d 0x00000001 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v DisableSmoothBackgroundGradient /d 0x00000000 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v HardwareRendering /d 0x00000001 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v LargerFonts /d 0x00000000 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v AssistedVsync /d 0x00000000 /f \
    && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v Vsync /d 0x00000000 /f \
    && wineserver -k' \
  # Clean up
  && rm -rf /var/lib/apt/lists/* \
  && rm -rf /var/cache/apt/* \
  && rm -rf /usr/share/doc/* \
  && rm -rf /usr/share/man/* \
  && rm -rf /root/.cache/* \
  && rm -rf /tmp/*

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Armageddon.Runner"]
