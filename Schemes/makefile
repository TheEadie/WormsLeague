default: all

# Arguments
OutputDir = .artifacts
UseDocker = true
DockerImageName = theeadie/wormscli

all: | schemes

schemes:
	@.build/build.sh $(UseDocker) $(OutputDir)

get-version:
	@.build/get-version.sh $(UseDocker)

help:
	@echo "Redgate Worms Schemes"
	@echo ""
	@echo "Commands:"
	@echo "make              - Generate Scheme (.wsc) files for all files"
	@echo "make schemes      - Generate Scheme (.wsc) files for all files"
	@echo "make get-version  - Calculate the next version of the Schemes"
	@echo ""
	@echo "Options:"
	@echo "UseDocker=<true|false> - (default:true)              - Should docker be used to get non-dotnet build tools"
	@echo "OutputDir=<path>       - (default:.artifacts)        - The path the CLI will be published to"
	@echo ""
	@echo "For release details see make release-help"

# Release Arguments
GitHubRepo = TheEadie/WormsLeague
GitHubAuthToken = empty

release: | release-github

release-github:
	@.build/release-github.sh $(GitHubAuthToken) $(GitHubRepo) "$(OutputDir)"

release-help:
	@echo "Worm CLI - Release"
	@echo ""
	@echo "Commands:"
	@echo "make release             - Release to all locations"
	@echo "make release-github      - Release to GitHub"
	@echo ""
	@echo "Options:"
	@echo "GitHubAuthToken=<token>    - (default:unspecified)          - A token used to authenticate with GitHub"
	@echo "GitHubRepo=<owner/repo>    - (default:TheEadie/WormsLeague) - The GitHub repo to release to"