default: all

# Arguments
UseDocker = true
DockerImageName = theeadie/worms-gateway

all: | docker

docker:
	@.build/build.sh $(UseDocker) $(DockerImageName)

get-version:
	@.build/get-version.sh $(UseDocker)

clean:
	@dotnet clean

build:
	@dotnet build

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
	@echo "DockerImageName=<name> - (default:theeadie/worms-gateway) - The name given to the docker image"