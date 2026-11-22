# Security (UI + REST API)

## REST API (`00EmpireOneRestAPIITJ`)

| Control | Details |
|---------|---------|
| **Headers** | `SecurityHeadersModule` — `X-Content-Type-Options`, `X-Frame-Options`, CSP, etc. |
| **HTTPS** | `RequireHttps=true` in **Release** publish (`Web.Release.config`) |
| **Swagger** | `EnableSwagger=false` in Release (404 on `/swagger/*`). When enabled, `SwaggerGateModule` enforces HTTP Basic Auth (`SwaggerUser` / `SwaggerPassword`); set `SwaggerAllowLocalWithoutAuth=true` only for local dev |
| **Session tokens** | After login, `sessionToken` is returned; send as `Authorization: Bearer {token}` |
| **Protected endpoints** | Subscription checkout/create require `[RequireItjSession]` |
| **Config** | Set a strong `SessionSigningKey` in `Web.config` before production (not `CHANGE_ME...`) |

## Angular UI (`ItechJump`)

| Control | Details |
|---------|---------|
| **Route guards** | `authGuard` on subscribe, scores, review, tests, your-question |
| **HTTP** | `authInterceptor` attaches Bearer token from `sessionStorage` |
| **Logoff** | Clears session token, auth cookies, and display storage |
| **IIS headers** | `web.config` custom security headers on static deploy; CSP allows `https://js.stripe.com` and Stripe `connect-src` hosts |
| **Prod API path** | Direct `https://api.itechjump.com` (`useSameOriginApi: false`); CORS allows `https://itechjump.com` |

## Local dev

- `SessionSigningKey` must be set (non-`CHANGE_ME`) for login to return `sessionToken` and for protected API POSTs.
- Use the same key in dev `Web.config` as you will use after first production deploy (or subscription calls return 401).