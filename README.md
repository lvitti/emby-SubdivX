# emby-SubdivX

SubdivX subtitle provider plugin for Emby. It searches and downloads Spanish subtitles for Movies and TV Shows from SubdivX.

## SubX-Api Token

- English: `docs/get-token.md`
- Español: `docs/get-token.es.md`

## DEV: Run Emby with Docker Compose

Spin up a local Emby server using the provided compose file.

Prerequisites
- Docker and Docker Compose installed
- Ensure the plugin binary exists at `Docker/plugins/SubdivX.dll` (mounted into Emby)

Start Emby

```bash
# From the repo root
docker compose up -d

# Follow logs
docker compose logs -f emby

# Stop and remove containers
docker compose down
```

Setup
- Place test media under `Docker/media` (mapped to `/media` in Emby).
- Open Emby at `http://localhost:8096` and complete the initial setup.
- The plugin DLL at `Docker/plugins/SubdivX.dll` is mounted into Emby automatically.

Notes
- Configure display/options from the plugin settings page in Emby.
