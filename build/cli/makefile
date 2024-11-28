include build/cli/config.mk

version:: cli.version
package:: cli.package
build:: cli.build
test:: cli.test

## Version

cli_VERSION = $(shell build/version.sh $(cli_NEXT_VERSION) $(cli_TAG_PREFIX) 2> /dev/null)
cli_VERSION_MAJOR = $(word 1,$(subst ., ,$(cli_VERSION)))
cli_VERSION_MINOR = $(word 2,$(subst ., ,$(cli_VERSION)))
cli_VERSION_PATCH = $(word 2,$(subst ., ,$(cli_VERSION)))

cli.version:
	@echo "Version: cli - $(cli_VERSION)"
	@echo ""

## Build

cli.build: cli.build.exe

cli.build.exe: cli.version
	@dotnet build "src/Worms.Cli/Worms.Cli.csproj"
	@echo ""

cli.build.docker: cli.version
	@VERSION=$(cli_VERSION) docker buildx bake -f docker-bake.hcl -f build/cli/docker-bake.hcl package
	@echo ""

## Test

cli.test: cli.test.unit

cli.test.unit: cli.build.exe
	@dotnet test
	@echo ""

## Package

cli.package: | cli.package.windows cli.package.linux cli.package.alpine cli.package.docker

cli.package.windows: cli.version
	@rm -rf .artifacts/win-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r win-x64 \
            -o .artifacts/win-x64 \
            --self-contained true \
            -p:PublishTrimmed=true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@cp src/Worms.Cli/*.ps1 .artifacts/win-x64/
	@echo $(cli_VERSION) > .artifacts/win-x64/version.txt
	@echo ""

cli.package.linux: cli.version
	@rm -rf .artifacts/linux-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r linux-x64 \
            -o .artifacts/linux-x64 \
            --self-contained true \
            -p:PublishTrimmed=true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@echo $(cli_VERSION) > .artifacts/linux-x64/version.txt
	@echo ""

cli.package.alpine: cli.version
	@rm -rf .artifacts/linux-musl-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r linux-musl-x64 \
            -o .artifacts/linux-musl-x64 \
            --self-contained true \
            -p:PublishTrimmed=true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@echo $(cli_VERSION) > .artifacts/linux-musl-x64/version.txt
	@echo ""

cli.package.docker: cli.package.alpine cli.build.docker

## Release

cli.release: cli.release.github cli.release.dockerhub

cli.release.github: cli.version
	@build/release-github.sh "CLI v$(cli_VERSION)" "cli/v$(cli_VERSION)" $(GitHubAuthToken) $(GitHubRepo) ".artifacts/win-x64"
	@echo ""

cli.release.dockerhub: cli.build.docker cli.version
	@VERSION=$(cli_VERSION) docker buildx bake -f docker-bake.hcl -f build/cli/docker-bake.hcl package --push
	@echo ""

cli.release.wormshub: cli.version
	@build/release-wormshub.sh $(WormsHubClientId) $(WormsHubClientSecret) $(cli_VERSION)
	@echo ""
