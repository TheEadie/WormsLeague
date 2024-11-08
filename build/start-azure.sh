#!/bin/bash
set pipefail -euo

pulumi up --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test --yes --skip-preview
JDBC_URL=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-jdbc --show-secrets)
USER=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-user --show-secrets)
PASSWORD=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-password --show-secrets)

flyway info migrate info \
    -url="$JDBC_URL" -user="$USER" -password="$PASSWORD" \
    -workingDirectory=./src/database
