#!/bin/bash
set pipefail -euo

pulumi up --cwd ./deployment/worms.davideadie.dev/ --stack test --yes --skip-preview
JDBC_URL=$(pulumi stack output --cwd ./deployment/worms.davideadie.dev/ --stack test database-jdbc --show-secrets)
USER=$(pulumi stack output --cwd ./deployment/worms.davideadie.dev/ --stack test database-user --show-secrets)
PASSWORD=$(pulumi stack output --cwd ./deployment/worms.davideadie.dev/ --stack test database-password --show-secrets)

flyway info migrate info \
    -url="$JDBC_URL" -user="$USER" -password="$PASSWORD" \
    -locations="filesystem:./src/database/migrations" \
    -configFiles=./src/database/flyway.conf \
    -reportEnabled=false