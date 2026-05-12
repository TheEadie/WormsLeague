build:: web.build

web.build:
	@cd src/Worms.Hub.Web && npm ci
	@cd src/Worms.Hub.Web && npx vite build
	@echo ""

web.lint:
	@cd src/Worms.Hub.Web && npx eslint src
	@cd src/Worms.Hub.Web && npx tsc -b
	@cd src/Worms.Hub.Web && npx prettier --check src
	@echo ""

web.test:
	@cd src/Worms.Hub.Web && npx vitest run
	@echo ""
