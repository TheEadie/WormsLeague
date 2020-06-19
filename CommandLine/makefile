default: all

# Arguments
OutputDir = .artifacts
UseDocker = true

all: | windows linux alpine

windows:
	@.build/publish.sh $(UseDocker) $(OutputDir) "win-x64"

linux:
	@.build/publish.sh $(UseDocker) $(OutputDir) "linux-x64"

alpine:
	@.build/publish.sh $(UseDocker) $(OutputDir) "linux-musl-x64"

get-version:
	@.build/get-version.sh $(UseDocker)

clean:
	@dotnet clean

build:
	@dotnet build

docker: | alpine docker-build

docker-build:
	@.build/docker.sh "$(OutputDir)/linux-musl-x64"

help:
	@echo "Worm CLI"
	@echo ""
	@echo "Commands:"
	@echo "make              - Build and publish the CLI for all platforms"
	@echo "make windows      - Build and publish the CLI for windows"
	@echo "make linux        - Build and publish the CLI for linux"
	@echo "make docker       - Package the linux build into a docker container"
	@echo "make get-version  - Calculate the next version of the CLI"
	@echo "make build        - Build the CLI for the local platform"
	@echo "make clean        - Clean the local bin directories"
	@echo ""
	@echo "Options:"
	@echo "UseDocker=<true|false> - (default:true)       - Should docker be used to get non-dotnet build tools"
	@echo "OutputDir=<path>       - (default:.artifacts) - The path the CLI will be published to"
	@echo ""
	@echo "For release details see make release-help"

# Release Arguments
GitHubRepo = TheEadie/WormsLeague
GitHubAuthToken = empty
DockerHubToken = empty

release: | release-github release-dockerhub

release-github:
	@.build/release.sh $(GitHubAuthToken) $(GitHubRepo) "$(OutputDir)/win-64"

release-dockerhub:
	@.build/release-dockerhub.sh $(DockerHubAuthToken)

release-help:
	@echo "Worm CLI - Release"
	@echo ""
	@echo "Commands:"
	@echo "make release             - Release to all locations"
	@echo "make release-github      - Release to GitHub"
	@echo "make release-dockerhub      - Release to DockerHub"
	@echo ""
	@echo "Options:"
	@echo "ReleaseDir=<path>        - (default:.artifacts/win-x64)   - The path containing the build to be released"
	@echo "GitHubAuthToken=<token>  - (default:unspecified)          - A token used to authenticate with GitHub"
	@echo "GitHubRepo=<owner/repo>  - (default:TheEadie/WormsLeague) - The GitHub repo to release to"