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
FROM ubuntu:26.04@sha256:5e275723f82c67e387ba9e3c24baa0abdcb268917f276a0561c97bef9450d0b4 AS runtime

RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  ca-certificates \
  wget \
  gnupg \
  libicu-dev \
  && rm -rf /var/lib/apt/lists/*

# Add Wine repo key and source
RUN mkdir -p /etc/apt/keyrings \
  && wget -nc https://dl.winehq.org/wine-builds/winehq.key -O /etc/apt/keyrings/winehq.asc \
  && echo "deb [signed-by=/etc/apt/keyrings/winehq.asc] https://dl.winehq.org/wine-builds/ubuntu/ resolute main" \
    > /etc/apt/sources.list.d/winehq.list

# AMD 32-bit deps for Wine
RUN dpkg --add-architecture i386 \
  && apt-get update \
  && apt-get install -y --install-recommends winehq-stable xvfb \
  && rm -rf /var/lib/apt/lists/*

# Create WINEPREFIX
RUN WINEDLLOVERRIDES="mscoree,mshtml=" xvfb-run wineboot -i \
  && wineserver -k \
  && rm -f /tmp/.X*-lock

# Copy game installation directory

# Some settings for WA
RUN xvfb-run sh -c '\
  wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v WineCompatibilitySuggested /d 0x7FFFFFFF /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v WindowedMode /d 0x00000001 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v DetailLevel /d 0x00000005 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v WindowedMode /d 0x00000001 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v PinnedChatLines /d 0x00000007 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v InfoTransparency /d 0x00000001 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v InfoSpy /d 0x00000001 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v DisableSmoothBackgroundGradient /d 0x00000000 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v HardwareRendering /d 0x00000001 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v LargerFonts /d 0x00000000 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v AssistedVsync /d 0x00000000 /f \
  && wine reg add "HKEY_CURRENT_USER\Software\Team17SoftwareLTD\WormsArmageddon\Options" /t REG_DWORD /v Vsync /d 0x00000000 /f \
  && wineserver -k' \
  && rm -f /tmp/.X*-lock

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["./Worms.Hub.Armageddon.Runner"]
