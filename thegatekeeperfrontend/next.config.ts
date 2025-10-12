// @ts-check
const { PHASE_DEVELOPMENT_SERVER } = require('next/constants')

// @ts-ignore
module.exports = (phase, { defaultConfig }) => {
  if (phase === PHASE_DEVELOPMENT_SERVER) {
    return {
      env: {
      },
      devIndicators: false,
      async rewrites() {
        return [
          {
            source: '/api/:path*',
            destination: 'http://localhost:8891/api/:path*',
          },
          {
            source: '/backendUpdate/:path*',
            destination: 'http://localhost:8891/backendUpdate/:path*',
          }
        ];
      },
    }
  }

  return {
    env: {
      urlPrefix: "/gatekeeper"
    },
    devIndicators: false,
    assetPrefix: "/gatekeeper",
    output: 'export'
  }
}
