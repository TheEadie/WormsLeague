build:: web.build
test:: web.test

web.build:
	@cd src/Worms.Hub.Web && npm ci
	@cd src/Worms.Hub.Web && npx vite build
	@echo ""

web.test: web.lint

web.lint:
	@cd src/Worms.Hub.Web && npx eslint src
	@cd src/Worms.Hub.Web && npx tsc --noEmit
	@cd src/Worms.Hub.Web && npx prettier --check src
	@echo ""
