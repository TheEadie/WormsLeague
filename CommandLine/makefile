default: all

# Arguments
OutputDir = .artifacts
UseDocker = true
DockerImageName = theeadie/wormscli

all: | windows linux alpine

windows:
	@.build/build.sh $(UseDocker) $(OutputDir) "win-x64"

linux:
	@.build/build.sh $(UseDocker) $(OutputDir) "linux-x64"

alpine:
	@.build/build.sh $(UseDocker) $(OutputDir) "linux-musl-x64"

get-version:
	@.build/get-version.sh $(UseDocker)

clean:
	@dotnet clean

build:
	@dotnet build

docker: | alpine docker-build

docker-build:
	@.build/build-docker.sh "$(OutputDir)/linux-musl-x64" $(DockerImageName)

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
	@echo "UseDocker=<true|false> - (default:true)              - Should docker be used to get non-dotnet build tools"
	@echo "OutputDir=<path>       - (default:.artifacts)        - The path the CLI will be published to"
	@echo "DockerImageName=<name> - (default:theeadie/wormscli) - The name given to the docker image"
	@echo ""
	@echo "For release details see make release-help"

# Release Arguments
GitHubRepo = TheEadie/WormsLeague
GitHubAuthToken = empty
DockerHubUsername = theeadie
DockerHubAuthToken = empty

release: | release-github release-dockerhub

release-github:
	@.build/release-github.sh $(GitHubAuthToken) $(GitHubRepo) "$(OutputDir)/win-x64"

release-dockerhub:
	@.build/release-dockerhub.sh $(DockerHubUsername) $(DockerHubAuthToken) "$(OutputDir)/linux-musl-x64" $(DockerImageName)

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
	@echo "DockerHubUsername=<name>   - (default:theeadie)             - The username to use to authenticate with DockerHub"
	@echo "DockerHubAuthToken=<token> - (default:unspecified)          - A token used to authenticate with DockerHub"