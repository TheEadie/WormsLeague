#### Build ####
FROM node:24-alpine@sha256:d1b3b4da11eefd5941e7f0b9cf17783fc99d9c6fc34884a665f40a06dbdfc94f AS build
WORKDIR /repo/src/Worms.Hub.Web

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json ./
RUN npm ci

COPY src/Worms.Hub.Web/. .
RUN npm run build

#### Runtime ####
FROM nginx:alpine@sha256:5616878291a2eed594aee8db4dade5878cf7edcb475e59193904b198d9b830de
COPY --from=build /repo/.artifacts/web /usr/share/nginx/html
COPY build/web/nginx.conf /etc/nginx/conf.d/default.conf
