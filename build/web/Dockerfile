#### Build ####
FROM node:24-alpine@sha256:d1b3b4da11eefd5941e7f0b9cf17783fc99d9c6fc34884a665f40a06dbdfc94f AS build
WORKDIR /repo/src/Worms.Hub.Web

COPY src/Worms.Hub.Web/package.json src/Worms.Hub.Web/package-lock.json ./
RUN npm ci

COPY src/Worms.Hub.Web/. .
RUN npm run build

#### Runtime ####
FROM nginx:alpine@sha256:feb6f75a08aa55b44576f98c15b8859819ecf54f3e4d2157f42c2d01cb58a3d2
COPY --from=build /repo/.artifacts/web /usr/share/nginx/html
COPY build/web/nginx.conf /etc/nginx/conf.d/default.conf
