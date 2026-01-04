/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    NEXT_PUBLIC_API_GATEWAY_URL: process.env.NEXT_PUBLIC_API_GATEWAY_URL || 'http://localhost:5000',
  },
  // Output standalone for Docker deployment
  output: 'standalone',
}

module.exports = nextConfig

