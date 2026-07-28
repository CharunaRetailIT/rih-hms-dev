# syntax=docker/dockerfile:1
# ─────────────────────────────────────────────────────────────────────────────
# RIT HMS Web — Next.js 15 standalone build. Build context = apps/web.
#   docker build -t rit-hms-web apps/web
# Debian (slim) rather than alpine so native deps (sharp) install cleanly.
# pnpm pinned to match CI + the v9.0 lockfile.
# ─────────────────────────────────────────────────────────────────────────────
FROM node:20-slim AS base
ENV PNPM_HOME=/pnpm PATH=/pnpm:$PATH NEXT_TELEMETRY_DISABLED=1
RUN npm install -g pnpm@10.34.1
WORKDIR /app

FROM base AS deps
COPY package.json pnpm-lock.yaml .npmrc pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

FROM base AS build
COPY --from=deps /app/node_modules ./node_modules
COPY . .
# `output: 'standalone'` (next.config.mjs) emits .next/standalone/server.js.
# Ensure ./public exists even when the app ships no static assets.
RUN mkdir -p public && pnpm build

FROM node:20-slim AS runtime
WORKDIR /app
ENV NODE_ENV=production NEXT_TELEMETRY_DISABLED=1 PORT=3000 HOSTNAME=0.0.0.0
RUN groupadd -g 1001 nodejs && useradd -u 1001 -g nodejs -m nextjs
COPY --from=build --chown=nextjs:nodejs /app/public ./public
COPY --from=build --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=build --chown=nextjs:nodejs /app/.next/static ./.next/static
USER nextjs
EXPOSE 3000
# Node 20 has a global fetch — no curl/wget needed in the image.
HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=10 \
  CMD node -e "fetch('http://localhost:3000/').then(r=>process.exit(r.ok?0:1)).catch(()=>process.exit(1))"
CMD ["node", "server.js"]
