FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json .
RUN npm install
COPY . .
RUN npm run build -- --configuration production

# ── Runtime: unprivileged nginx (runs as uid 101, no root required) ────────────
FROM nginxinc/nginx-unprivileged:alpine
COPY --from=build /app/dist/taskmanager-front/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -q --spider http://localhost:8080/ || exit 1
