start:
	@docker-compose build --build-arg VERSION=0.0.1
	@docker-compose up -d

stop:
	@docker-compose down
