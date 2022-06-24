IMAGE_NAME := theeadie/worms-server-gateway
NEXT_VERSION := 0.1
TAG_PREFIX := server/gateway/v

GITHUB_REPO := theeadie/WormsLeague
GITHUB_AUTH_TOKEN := empty

VERSION := $(shell ../build/version.sh $(NEXT_VERSION) $(TAG_PREFIX))
VERSION_MAJOR := $(word 1,$(subst ., ,$(VERSION)))
VERSION_MINOR := $(word 2,$(subst ., ,$(VERSION)))
VERSION_PATCH := $(word 2,$(subst ., ,$(VERSION)))

.PHONY: default build test

## Build
default: publish-local

publish-local:
	@docker buildx build . \
		-t $(IMAGE_NAME):$(VERSION)-dev \
		--build-arg VERSION=$(VERSION) \
		--load

build-local:
	@docker buildx build . \
		-t $(IMAGE_NAME):$(VERSION)-build \
		--build-arg VERSION=$(VERSION) \
		--target build \
		--load \
		--cache-from=type=gha,scope=build-local \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=build-local

build:
	@docker buildx build . \
		-t $(IMAGE_NAME):$(VERSION)-build \
		--build-arg VERSION=$(VERSION) \
		--target build \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=build

test: build-local
	@docker run --rm -i $(IMAGE_NAME):$(VERSION)-build dotnet test

publish:
	@docker buildx build . \
		-t $(IMAGE_NAME):$(VERSION) \
		--build-arg VERSION=$(VERSION) \
		--cache-from=type=gha,scope=publish \
		--cache-from=type=gha,scope=build \
		--cache-to=type=gha,mode=max,scope=publish

start:
	@docker-compose build --build-arg VERSION=$(VERSION)
	@docker-compose up -d

stop:
	@docker-compose down

version:
	@echo $(VERSION)

## Release
release: | docker-push

docker-push:
	@docker buildx build . \
		-t $(IMAGE_NAME):latest \
		-t $(IMAGE_NAME):$(VERSION) \
		-t $(IMAGE_NAME):$(VERSION_MAJOR) \
		-t $(IMAGE_NAME):$(VERSION_MAJOR).$(VERSION_MINOR) \
		--build-arg VERSION=$(VERSION) \
		--push \
		--cache-from=type=gha,scope=publish \
		--cache-to=type=gha,mode=max,scope=publish