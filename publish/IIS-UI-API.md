# Legacy: same-origin `/api` proxy (deprecated)

API TLS on **`https://api.itechjump.com`** is the supported production setup.

Prod UI now uses **`useSameOriginApi: false`** and calls the API host directly. You do **not** need ARR, loopback port **15081**, or `/api` proxy rules on the UI site.

If you previously added a `127.0.0.1:15081` binding or ARR rules, you can remove them after deploying the current `publish/ui` (`web.config` has SPA routing only).

See `publish/README.md`.