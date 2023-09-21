include build/cli/config.mk

package:: cli.package
build:: cli.build
test:: cli.test

cli_VERSION := $(shell build/version.sh $(cli_NEXT_VERSION) $(cli_TAG_PREFIX))
cli_VERSION_MAJOR := $(word 1,$(subst ., ,$(cli_VERSION)))
cli_VERSION_MINOR := $(word 2,$(subst ., ,$(cli_VERSION)))
cli_VERSION_PATCH := $(word 2,$(subst ., ,$(cli_VERSION)))

## Build

cli.build: cli.build.exe

cli.build.exe:
	@dotnet build "src/Worms.Cli/Worms.Cli.csproj"

cli.build.docker:
	@docker build . \
		-f "build/cli/Dockerfile" \
		-t $(cli_IMAGE_NAME):latest \
		-t $(cli_IMAGE_NAME):$(cli_VERSION) \
		-t $(cli_IMAGE_NAME):$(cli_VERSION_MAJOR) \
		-t $(cli_IMAGE_NAME):$(cli_VERSION_MAJOR).$(cli_VERSION_MINOR)

## Test

cli.test: cli.test.unit

cli.test.unit: cli.build.exe
	@dotnet test "src/Worms.Armageddon.Files.Tests/Worms.Armageddon.Files.Tests.csproj"

## Package

cli.package: | cli.package.windows cli.package.linux cli.package.alpine cli.package.docker

cli.package.windows:
	@rm -rf .artifacts/win-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r win-x64 \
            -o .artifacts/win-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@cp *.ps1 .artifacts/win-x64/
	@echo $(cli_VERSION) > .artifacts/win-x64/version.txt

cli.package.linux:
	@rm -rf .artifacts/linux-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r linux-x64 \
            -o .artifacts/linux-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@echo $(cli_VERSION) > .artifacts/linux-x64/version.txt

cli.package.alpine:
	@rm -rf .artifacts/linux-musl-x64
	@dotnet publish \
            "src/Worms.Cli/Worms.Cli.csproj" \
            -c Release \
            -r linux-musl-x64 \
            -o .artifacts/linux-musl-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(cli_VERSION) \
            /p:DebugType=none
	@echo $(cli_VERSION) > .artifacts/linux-musl-x64/version.txt

cli.package.docker: cli.package.alpine cli.build.docker

## Release

cli.release: cli.release.github cli.release.dockerhub

cli.release.github:
	@build/release-github.sh "CLI v$(cli_VERSION)" "cli/v$(cli_VERSION)" $(GitHubAuthToken) $(GitHubRepo) ".artifacts/win-x64"

cli.release.dockerhub: cli.build.docker
	@docker push $(IMAGE_NAME) --all-tags
