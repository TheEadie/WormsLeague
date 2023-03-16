default: all

# Arguments
OUTPUT_DIR = .artifacts
IMAGE_NAME = theeadie/wormscli

NEXT_VERSION := 0.20
TAG_PREFIX := cli/v

VERSION := $(shell ../../build/version.sh $(NEXT_VERSION) $(TAG_PREFIX))
VERSION_MAJOR := $(word 1,$(subst ., ,$(VERSION)))
VERSION_MINOR := $(word 2,$(subst ., ,$(VERSION)))
VERSION_PATCH := $(word 2,$(subst ., ,$(VERSION)))

all: | windows linux alpine

windows:
	@rm -rf $(OUTPUT_DIR)/win-x64
	@dotnet publish \
            "src/Worms/worms.csproj" \
            -c Release \
            -r win-x64 \
            -o $(OUTPUT_DIR)/win-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(VERSION) \
            /p:DebugType=none
	@cp *.ps1 $(OUTPUT_DIR)/win-x64/
	@echo $(VERSION) > $(OUTPUT_DIR)/win-x64/version.txt

linux:
	@rm -rf $(OUTPUT_DIR)/linux-x64
	@dotnet publish \
            "src/Worms/worms.csproj" \
            -c Release \
            -r linux-x64 \
            -o $(OUTPUT_DIR)/linux-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(VERSION) \
            /p:DebugType=none
	@echo $(VERSION) > $(OUTPUT_DIR)/linux-x64/version.txt

alpine:
	@rm -rf $(OUTPUT_DIR)/linux-musl-x64
	@dotnet publish \
            "src/Worms/worms.csproj" \
            -c Release \
            -r linux-musl-x64 \
            -o $(OUTPUT_DIR)/linux-musl-x64 \
            --self-contained true \
            /p:PublishSingleFile=true \
            /p:Version=$(VERSION) \
            /p:DebugType=none
	@echo $(VERSION) > $(OUTPUT_DIR)/linux-musl-x64/version.txt

version:
	@echo $(VERSION)

clean:
	@dotnet clean

build:
	@dotnet build

docker: | alpine docker-build

docker-build:
	@docker build . \
		-t $(IMAGE_NAME):latest \
		-t $(IMAGE_NAME):$(VERSION) \
		-t $(IMAGE_NAME):$(VERSION_MAJOR) \
		-t $(IMAGE_NAME):$(VERSION_MAJOR).$(VERSION_MINOR)

help:
	@echo "Worm CLI"
	@echo ""
	@echo "Commands:"
	@echo "make              - Build and publish the CLI for all platforms"
	@echo "make windows      - Build and publish the CLI for windows"
	@echo "make linux        - Build and publish the CLI for linux"
	@echo "make docker       - Package the linux build into a docker container"
	@echo "make version      - Calculate the next version of the CLI"
	@echo "make build        - Build the CLI for the local platform"
	@echo "make clean        - Clean the local bin directories"
	@echo ""
	@echo "Options:"
	@echo "OUTPUT_DIR=<path>      - (default:.artifacts)        - The path the CLI will be published to"
	@echo "IMAGE_NAME=<name>      - (default:theeadie/wormscli) - The name given to the docker image"
	@echo ""
	@echo "For release details see make release-help"

# Release Arguments
GitHubRepo = TheEadie/WormsLeague
GitHubAuthToken = empty

release: | release-github release-dockerhub

release-github:
	@../../build/release-github.sh "CLI v$(VERSION)" "cli/v$(VERSION)" $(GitHubAuthToken) $(GitHubRepo) "$(OUTPUT_DIR)/win-x64"

release-dockerhub: docker-build
	@docker push $(IMAGE_NAME) --all-tags

release-help:
	@echo "Worm CLI - Release"
	@echo ""
	@echo "Commands:"
	@echo "make release             - Release to all locations"
	@echo "make release-github      - Release to GitHub"
	@echo "make release-dockerhub   - Release to DockerHub"
	@echo ""
	@echo "Options:"
	@echo "GitHubAuthToken=<token>    - (default:unspecified)          - A token used to authenticate with GitHub"
	@echo "GitHubRepo=<owner/repo>    - (default:TheEadie/WormsLeague) - The GitHub repo to release to"