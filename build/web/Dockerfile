#### Build ####
FROM node:22-alpine@sha256:8ea2348b068a9544dae7317b4f3aafcdc032df1647bb7d768a05a5cad1a7683f AS build
WORKDIR /repo/src/Worms.Hub.Web

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json ./
RUN npm ci

COPY src/Worms.Hub.Web/. .
RUN npm run build

#### Runtime ####
FROM nginx:alpine@sha256:5616878291a2eed594aee8db4dade5878cf7edcb475e59193904b198d9b830de
COPY --from=build /repo/.artifacts/web /usr/share/nginx/html
