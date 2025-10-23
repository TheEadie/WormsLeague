#!/bin/bash
set pipefail -euo

pulumi up --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test --yes --skip-preview

# Run Flyway to update database schema
JDBC_URL=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-jdbc --show-secrets)
USER=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-user --show-secrets)
PASSWORD=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test database-password --show-secrets)

flyway info migrate info \
    -url="$JDBC_URL" -user="$USER" -password="$PASSWORD" \
    -workingDirectory=./src/database

SOURCE_ACCOUNT="wormstest"
DESTINATION_ACCOUNT=$(pulumi stack output --cwd ./deployment/Worms.Hub.Infrastructure/ --stack test storage-account-name --show-secrets)

# Copy files to new storage account
az storage copy \
  --source-account-name $SOURCE_ACCOUNT \
  --source-share file-share \
  --destination-account-name $DESTINATION_ACCOUNT \
  --destination-share file-share \
  --recursive
