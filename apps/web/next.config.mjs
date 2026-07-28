/** @type {import('next').NextConfig} */
// The .NET API base URL. In dev it's localhost:5000; in a container it's the
// API service (set API_BASE_URL=http://api:5000). The browser still calls /api/*
// relative paths — those are routed to the API by the reverse proxy (Caddy) in a
// deployed env, or by this rewrite when hitting the Next server directly.
const apiBase = process.env.API_BASE_URL ?? 'http://localhost:5000';

const nextConfig = {
  // Emit a self-contained server bundle (.next/standalone) for a slim Docker image.
  output: 'standalone',
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${apiBase}/api/:path*`,
      },
    ];
  },
};
export default nextConfig;
