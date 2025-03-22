// @ts-check
const { PHASE_DEVELOPMENT_SERVER } = require('next/constants')

// @ts-ignore
module.exports = (phase, { defaultConfig }) => {
  if (phase === PHASE_DEVELOPMENT_SERVER) {
    return {
      devIndicators: false,
      async rewrites() {
        return [
          {
            source: '/api/:path*',
            destination: 'http://localhost:8891/api/:path*',
          },
          {
            source: '/timedOutUserVote/:path*',
            destination: 'http://localhost:8891/timedOutUserVote/:path*',
          }
        ];
      },
    }
  }

  return {
    devIndicators: false,
    assetPrefix: "/gateKeeper",
    output: 'export'
  }
}
