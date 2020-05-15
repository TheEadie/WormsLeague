default: publish

# Arguments
OutputDir = .artifacts
UseDocker = true

publish:
	@.build/publish.sh $(UseDocker) $(OutputDir)

get-version:
	@.build/get-version.sh $(UseDocker)

clean:
	@dotnet clean

build:
	@dotnet build

help:
	@echo "Worm CLI"
	@echo ""
	@echo "Commands:"
	@echo "make publish \t\t - Build, Publish and minify the CLI for all platforms"
	@echo "make get-version \t - Calculate the next version of the CLI"
	@echo "make build \t\t - Build the CLI for the local platform"
	@echo "make clean \t\t - Clean the local bin directories"
	@echo ""
	@echo "Options:"
	@echo "UseDocker=<true|false> \t - (default:true) \t - Should docker be used to get non-dotnet build tools"
	@echo "OutputDir=<path> \t - (default:.artifacts)  - The path the CLI will be published to"