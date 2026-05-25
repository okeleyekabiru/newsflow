/** @type {import('next').NextConfig} */
const nextConfig = {
  // Required for multi-stage Docker build (copies only what's needed to run)
  output: 'standalone',
  // Proxy all /api/* and /hubs/* requests to the .NET backend.
  // INTERNAL_API_URL is a *server-side* runtime variable so the destination
  // can differ between Docker (http://api:8080) and local dev (http://localhost:5000)
  // without rebuilding the image.  It is NOT prefixed NEXT_PUBLIC_ so it is
  // never exposed to the browser.
  async rewrites() {
    const backend = process.env.INTERNAL_API_URL ?? 'http://localhost:5000';
    return [
      { source: '/api/:path*',  destination: `${backend}/api/:path*`  },
      { source: '/hubs/:path*', destination: `${backend}/hubs/:path*` },
    ];
  },
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: '**.r2.cloudflarestorage.com' },
      { protocol: 'https', hostname: 'images.pexels.com' },
    ],
  },
};

export default nextConfig;
