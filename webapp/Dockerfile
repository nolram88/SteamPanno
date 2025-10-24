# Deployable Bun image
FROM oven/bun:1 as base
WORKDIR /app

# Install deps
COPY webapp/package.json webapp/tsconfig.json ./
RUN bun install --frozen-lockfile

# Copy source
COPY webapp ./

ENV NODE_ENV=production
EXPOSE 3000
CMD ["bun","src/server.ts"]

