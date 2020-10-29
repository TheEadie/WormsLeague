default: all

# Arguments
OutputDir = .artifacts
UseDocker = true
DockerImageName = theeadie/worms-gateway

all: | linux

linux:
	@.build/build.sh $(UseDocker) $(OutputDir) "linux-x64"

get-version:
	@.build/get-version.sh $(UseDocker)

clean:
	@dotnet clean

build:
	@dotnet build

docker: | linux docker-build

docker-build:
	@.build/build-docker.sh "$(OutputDir)/linux-x64" $(DockerImageName)

help:
	@echo "Worm Server"
	@echo ""
	@echo "Commands:"
	@echo "make              - Build and publish the Server for all platforms"
	@echo "make docker       - Package the linux build into a docker container"
	@echo "make get-version  - Calculate the next version of the CLI"
	@echo "make build        - Build for the local platform"
	@echo "make clean        - Clean the local bin directories"
	@echo ""
	@echo "Options:"
	@echo "UseDocker=<true|false> - (default:true)                   - Should docker be used to get non-dotnet build tools"
	@echo "OutputDir=<path>       - (default:.artifacts)             - The path the CLI will be published to"
	@echo "DockerImageName=<name> - (default:theeadie/worms-gateway) - The name given to the docker image"