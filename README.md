# emby-SubdivX

SubdivX subtitle provider plugin for Emby. It searches and downloads Spanish subtitles for Movies and TV Shows from SubdivX.

## Important: FlareSolverr required

- You must have a running FlareSolverr instance. The plugin defaults to `http://localhost:8191/v1`.
- When running Emby inside Docker for local development, set the plugin URL to `http://host.docker.internal:8191/v1` so the container can reach FlareSolverr running on your host.
- Project: https://github.com/FlareSolverr/FlareSolverr

## DEV: Run Emby + FlareSolverr with Docker Compose

Spin up a local Emby server and FlareSolverr together using the provided compose file.

Prerequisites
- Docker and Docker Compose installed
- Ensure the plugin binary exists at `Docker/plugins/SubdivX.dll` (mounted into Emby)

Start both services (recommended)

```bash
# From the repo root
docker compose up -d

# Follow logs
docker compose logs -f emby
docker compose logs -f flaresolverr

# Stop and remove containers
docker compose down
```

Setup
- Place test media under `Docker/media` (mapped to `/media` in Emby).
- Open Emby at `http://localhost:8096` and complete the initial setup.
- The plugin DLL at `Docker/plugins/SubdivX.dll` is mounted into Emby automatically.
- In the plugin settings, set FlareSolverr URL to `http://flaresolverr:8191/v1`.

Alternative: run containers manually

```bash
# FlareSolverr
docker run -d \
  --name flaresolverr \
  -p 8191:8191 \
  ghcr.io/flaresolverr/flaresolverr:latest

# Emby (mount media and plugin)
docker run -d \
  --name emby-dev \
  -p 8096:8096 \
  -v "$PWD/Docker/media:/media" \
  -v "$PWD/Docker/plugins/SubdivX.dll:/system/plugins/SubdivX.dll" \
  --add-host=host.docker.internal:host-gateway \
  emby/embyserver:latest

# In Emby plugin settings, set FlareSolverr URL to:
# - If using Docker Compose: http://flaresolverr:8191/v1
# - If running FlareSolverr on host: http://host.docker.internal:8191/v1
```

Notes
- Configure display/options from the plugin settings page in Emby.
