# syntax=docker/dockerfile:1
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
# npm install используется, потому что ЛР4 добавляет frontend-test зависимости.
RUN npm install --no-audit --no-fund
COPY . ./
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=15s --timeout=5s --retries=5 --start-period=5s \
  CMD wget -qO- http://localhost/healthz >/dev/null 2>&1 || exit 1
