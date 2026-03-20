// @ts-check
const { PHASE_DEVELOPMENT_SERVER } = require('next/constants')

// @ts-ignore
module.exports = (phase, { defaultConfig }) => {
  if (phase === PHASE_DEVELOPMENT_SERVER) {
    return {
      env: {
        NEXT_PUBLIC_OIDC_AUTHORITY: "http://localhost:8892/realms/thegatekeeper",
        NEXT_PUBLIC_OIDC_REDIRECT_URI: "http://localhost:3000/",
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
      urlPrefix: "",
      NEXT_PUBLIC_OIDC_AUTHORITY: "https://auth.bergeral.me/realms/thegatekeeper",
      NEXT_PUBLIC_OIDC_REDIRECT_URI: "https://bergeral.me/",
    },
    devIndicators: false,
    assetPrefix: "",
    output: 'export'
  }
}
