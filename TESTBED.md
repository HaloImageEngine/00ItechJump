# Local testbed (UI + REST API)

Runs **ItechJump** against **00EmpireOneRestAPIITJ** on your machine.

| Service | URL |
|--------|-----|
| Angular UI | http://localhost:4200 (proxies `/api` and `/QandA` to the API) |
| REST API (direct) | http://localhost:64150 |

## Quick start

From this folder:

```powershell
.\Run-Testbed.ps1
```

Or double-click `Run-Testbed.bat`.

The UI uses a **dev proxy** so the browser only talks to `localhost:4200` — no HTTPS certificate step for normal use.

If the UI cannot reach the API, confirm the API responds: http://localhost:64150/api/status/ping

## VS Code / Cursor

1. **Terminal → Run Task… → Run Testbed** (starts API, then UI).
2. Or **Run and Debug → Testbed UI (Chrome)** (same tasks + opens browser).

## UI only (API already running)

```powershell
cd ItechJump
npm run start:testbed
```

`start:testbed` swaps in `environment.testbed.ts` (`apiBaseUrl` is empty so calls are relative) and `proxy.conf.testbed.json` forwards to `http://localhost:64150`.

## Requirements

- Node.js + `npm install` in `ItechJump`
- IIS Express (Visual Studio or Build Tools)
- SQL/connection strings configured in the API `Web.config` (same as normal API debugging)