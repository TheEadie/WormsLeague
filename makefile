GITHUB_REPO := theeadie/WormsLeague
GITHUB_AUTH_TOKEN := empty

default: publish

start:
	@docker-compose build --build-arg VERSION=0.0.1
	@docker-compose up -d

stop:
	@docker-compose down

include build/docker/makefile
include build/cli/makefile

