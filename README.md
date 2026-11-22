# 00ItechJump

Monorepo for **ITechJump** — Angular SPA + ASP.NET Web API.

| Folder | Project | GitHub (legacy) |
|--------|---------|-----------------|
| [`ItechJump/`](ItechJump/) | Angular UI (`itechjump.com`) | [ITechJump-mobile](https://github.com/HaloImageEngine/ITechJump-mobile) |
| [`00EmpireOneRestAPIITJ/`](00EmpireOneRestAPIITJ/) | REST API (`api.itechjump.com`) | [EmpireOneRestAPIITJ](https://github.com/HaloImageEngine/EmpireOneRestAPIITJ) |

## Build for production

```powershell
.\Publish-Prod.ps1
```

Output: `publish/ui` and `publish/api`. See [publish/README.md](publish/README.md).

## Local testbed

See [TESTBED.md](TESTBED.md) and `Run-Testbed.ps1`.

## Security

See [SECURITY.md](SECURITY.md). Do not commit `Web.config` with live connection strings or API keys — use local `Web.config` (gitignored).