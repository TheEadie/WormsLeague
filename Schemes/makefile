default: all

# Arguments
OUTPUT_DIR = .artifacts
IMAGE_NAME = theeadie/wormscli

NEXT_VERSION :=2020.0
TAG_PREFIX := schemes/v

VERSION := $(shell ../build/version.sh $(NEXT_VERSION) $(TAG_PREFIX))
VERSION_MAJOR := $(word 1,$(subst ., ,$(VERSION)))
VERSION_MINOR := $(word 2,$(subst ., ,$(VERSION)))
VERSION_PATCH := $(word 2,$(subst ., ,$(VERSION)))

all: | schemes

schemes:
	@rm -rf $(OUTPUT_DIR)
	@mkdir $(OUTPUT_DIR)
	@docker run -v $(PWD):/Schemes -v $(PWD)/$(OUTPUT_DIR):/.artifacts theeadie/wormscli create scheme "Uber.Coolest.Options.$(VERSION)" -f "/Schemes/Uber Coolest Options.txt" -r "/.artifacts"
	@docker run -v $(PWD):/Schemes -v $(PWD)/$(OUTPUT_DIR):/.artifacts theeadie/wormscli create scheme "Speed.Worms.$(VERSION)" -f "/Schemes/Speed Worms.txt" -r "/.artifacts"
	@echo $(VERSION) > $(OUTPUT_DIR)/version.txt

version:
	@echo $(VERSION)

help:
	@echo "Redgate Worms Schemes"
	@echo ""
	@echo "Commands:"
	@echo "make              - Generate Scheme (.wsc) files for all files"
	@echo "make schemes      - Generate Scheme (.wsc) files for all files"
	@echo "make version      - Calculate the next version of the Schemes"
	@echo ""
	@echo "Options:"
	@echo "OUTPUT_DIR=<path> - (default:.artifacts)        - The path the CLI will be published to"
	@echo ""
	@echo "For release details see make release-help"

# Release Arguments
GitHubRepo = TheEadie/WormsLeague
GitHubAuthToken = empty

release: | release-github

release-github:
	@../build/release-github.sh "Redgate Schemes v$(VERSION)" "schemes/v$(VERSION)" $(GitHubAuthToken) $(GitHubRepo) "$(OUTPUT_DIR)"

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