DOCKER_COMPONENTS := $(shell find build/docker/* -maxdepth 1 -mindepth 1 -name Dockerfile  | cut -f3,3 -d/)

.PHONY: version build test package

version:: $(addsuffix .version,$(DOCKER_COMPONENTS))
build:: $(addsuffix .build,$(DOCKER_COMPONENTS))
test:: $(addsuffix .test,$(DOCKER_COMPONENTS))
package:: $(addsuffix .package,$(DOCKER_COMPONENTS))

define make-docker-rules

include build/docker/$1/config.mk

.PHONY: $1.build $1.test $1.package

$1_VERSION := $$(shell build/version.sh $$($1_NEXT_VERSION) $$($1_TAG_PREFIX) 2> /dev/null)

## Build
$1.version:
	@echo "Version: $1 - $$($1_VERSION)"
	@echo ""

$1.build: $1.version
	@VERSION=$$($1_VERSION) docker buildx bake -f docker-bake.hcl -f build/docker/$1/docker-bake.hcl build
	@echo ""

$1.test: $1.version
	@VERSION=$$($1_VERSION) docker buildx bake -f docker-bake.hcl -f build/docker/$1/docker-bake.hcl test
	@echo ""

$1.package: $1.version
	@VERSION=$$($1_VERSION) docker buildx bake -f docker-bake.hcl -f build/docker/$1/docker-bake.hcl package
	@echo ""

## Release
$1.release: | $1.release.github $1.release.dockerhub

$1.release.github: $1.version
	@build/release-github.sh "Hub v$$($1_VERSION)" "$$($1_TAG_PREFIX)$$($1_VERSION)" $(GITHUB_AUTH_TOKEN) $(GITHUB_REPO)
	@echo ""

$1.release.dockerhub: $1.version
	@VERSION=$$($1_VERSION) docker buildx bake -f docker-bake.hcl -f build/docker/$1/docker-bake.hcl package --push
	@echo ""

endef
$(foreach component,$(DOCKER_COMPONENTS),$(eval $(call make-docker-rules,$(component))))
