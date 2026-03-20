# Cloudflare Tunnel + Keycloak Public Setup

How the application is exposed publicly via a Cloudflare Tunnel, and how Keycloak is configured to work behind it.

---

## Architecture Overview

```
Browser
  │
  ▼
Cloudflare (TLS termination)
  │
  ├── bergeral.me          → localhost:8891  (TheGateKeeper app)
  └── auth.bergeral.me     → localhost:8892  (Keycloak)
```

Cloudflare terminates HTTPS. Traffic arrives at the machine over plain HTTP via the tunnel daemon.

---

## 1. Cloudflare Tunnel

The tunnel runs as a **systemd service** (`cloudflared`).

### Service config location
```
/etc/cloudflared/config.yml
```

### Expected structure
```yaml
tunnel: <tunnel-name>
credentials-file: /root/.cloudflared/<tunnel-id>.json
ingress:
  - hostname: auth.bergeral.me
    service: http://localhost:8892
  - hostname: bergeral.me
    service: http://localhost:8891
  - service: http_status:404
```

> Do **not** use `~/.cloudflared/config.yml` — the systemd service reads from `/etc/cloudflared/`.

### DNS records

Each hostname needs a CNAME registered in Cloudflare pointing to the tunnel. To add one:

```bash
cloudflared tunnel route dns <tunnel-name> auth.bergeral.me
cloudflared tunnel route dns <tunnel-name> bergeral.me
```

Use `cloudflared tunnel list` to find your tunnel name if unsure.

### Restarting the tunnel

```bash
sudo systemctl restart cloudflared
systemctl is-active cloudflared   # should print: active
```

---

## 2. Keycloak (Docker)

Keycloak runs as a Docker container (`keycloak` service in `docker-compose.yml`) on port `8892`.

### Key environment variables

| Variable | Value | Purpose |
|---|---|---|
| `KC_HOSTNAME` | `https://auth.bergeral.me` | Public URL used in tokens and redirects |
| `KC_HOSTNAME_STRICT` | `false` | Allows admin access on the internal hostname too |
| `KC_PROXY_HEADERS` | `xforwarded` | Trusts `X-Forwarded-*` headers from Cloudflare |
| `KC_HTTP_ENABLED` | `true` | Allows plain HTTP internally (TLS is handled by Cloudflare) |

> `KC_PROXY_HEADERS: xforwarded` requires **Keycloak ≥ 21**. On older versions use `KC_PROXY: edge` instead.

### Admin credentials

Loaded at runtime from Docker secrets (files under `./secrets/`). Not stored in environment variables or source control.

---

## 3. Keycloak Admin Console — Client Configuration

After starting the stack, log into `https://auth.bergeral.me` and configure the OIDC client:

- **Realm:** `thegatekeeper`
- **Client ID:** `gateKeeperAppfrontEndClient`

| Field | Value |
|---|---|
| Valid Redirect URIs | `https://bergeral.me/*` |
| Valid Post Logout Redirect URIs | `https://bergeral.me/*` |
| Web Origins | `https://bergeral.me` |

Remove any leftover `localhost` entries.

---

## 4. Frontend OIDC Config

Located in `thegatekeeperfrontend/app/page.tsx`:

```ts
const oidcConfig = {
  authority: "https://auth.bergeral.me/realms/thegatekeeper",
  client_id: "gateKeeperAppfrontEndClient",
  redirect_uri: "https://bergeral.me/",
  response_type: "code",
  automaticSilentRenew: true,
  loadUserInfo: true,
};
```

---

## 5. Bringing the Stack Up/Down

```bash
# From the project root
docker compose up -d
docker compose down
```

Services started:
- `the-gate-keeper` — .NET backend on port `8891`
- `keycloak` — Keycloak on port `8892`
- `keycloak-db` — PostgreSQL (internal only, no exposed port)

---

## Secrets

All secrets live in the `./secrets/` directory as plain text files and are injected via Docker secrets. **Never commit this directory to source control.**

Required files:
- `api_key`
- `mongoDbConnectionString`
- `mongoDbUser`
- `mongoDbPassword`
- `discordWebhook`
- `keycloak_admin`
- `keycloak_admin_password`
