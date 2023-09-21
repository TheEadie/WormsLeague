DOCKER_COMPONENTS := $(shell find build/docker/* -maxdepth 1 -mindepth 1 -name Dockerfile  | cut -f3,3 -d/)

.PHONY: build test package

build:: $(addsuffix .build,$(DOCKER_COMPONENTS))
test:: $(addsuffix .test,$(DOCKER_COMPONENTS))
package:: $(addsuffix .package,$(DOCKER_COMPONENTS))

define make-docker-rules

include build/docker/$1/config.mk

.PHONY: $1.build $1.test $1.package

$1_VERSION := $$(shell build/version.sh $$($1_NEXT_VERSION) $$($1_TAG_PREFIX))
$1_VERSION_MAJOR := $$(word 1,$$(subst ., ,$$($1_VERSION)))
$1_VERSION_MINOR := $$(word 2,$$(subst ., ,$$($1_VERSION)))
$1_VERSION_PATCH := $$(word 3,$$(subst ., ,$$($1_VERSION)))

## Build
$1.build:
	@docker buildx build . \
		-f build/docker/$1/Dockerfile \
		-t $$($1_IMAGE_NAME):$$($1_VERSION)-build \
		--build-arg VERSION=$$($1_VERSION) \
		--target build \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=build

$1.test:
	@docker buildx build . \
		-f build/docker/$1/Dockerfile \
		-t $$($1_IMAGE_NAME):$$($1_VERSION)-build \
		--build-arg VERSION=$$($1_VERSION) \
		--target test \
		--cache-from=type=gha,scope=test \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=test

$1.package:
	@docker buildx build . \
		-f build/docker/$1/Dockerfile \
		-t $$($1_IMAGE_NAME):$$($1_VERSION) \
		--build-arg VERSION=$$($1_VERSION) \
		--cache-from=type=gha,scope=package \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=package

## Release
$1.release: | $1.release.github $1.release.dockerhub

$1.release.github:
	@build/release-github.sh "Hub v$$($1_VERSION)" "$$($1_TAG_PREFIX)$$($1_VERSION)" $(GITHUB_AUTH_TOKEN) $(GITHUB_REPO)

$1.release.dockerhub:
	@docker buildx build . \
		-f build/docker/$1/Dockerfile \
		-t $$($1_IMAGE_NAME):latest \
		-t $$($1_IMAGE_NAME):$$($1_VERSION) \
		-t $$($1_IMAGE_NAME):$$($1_VERSION_MAJOR) \
		-t $$($1_IMAGE_NAME):$$($1_VERSION_MAJOR).$$($1_VERSION_MINOR) \
		--build-arg VERSION=$$($1_VERSION) \
		--push \
		--cache-from=type=gha,scope=package \
		--cache-to=type=gha,mode=max,scope=package

endef
$(foreach component,$(DOCKER_COMPONENTS),$(eval $(call make-docker-rules,$(component))))
