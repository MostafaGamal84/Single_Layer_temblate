# Production Deploy (Somee - API + Front Together)

## 1) Build and publish
Run from project root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-production.ps1
```

This will:
- build Angular in production mode
- copy front files into `API/wwwroot`
- publish ASP.NET Core Release into `artifacts/publish-production`

## 2) Upload to Somee
- Upload all files from `artifacts/publish-production` to your Somee site root.
- Set startup command (if needed by your plan) to run the published app.

## 3) Config
- `API/appsettings.json` now includes:
  - production DB connection
  - `TokenKey`
  - CORS allowed origins
- Front production environment uses relative paths:
  - `apiBaseUrl: /api`
  - `hubUrl: /hubs/live-game`

## 4) Notes
- Keep `TokenKey` long and secret.
- If domain changes, update `Cors:AllowedOrigins`.
- Uploaded question cover images are stored under `wwwroot/uploads`.
