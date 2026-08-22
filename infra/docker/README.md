# Docker deployment

CI builds and pushes three images to GHCR:

| Component | Image                                             | Workflow                |
|-----------|----------------------------------------------------|--------------------------|
| Site      | `ghcr.io/sandermvanvliet/roadcaptain-site:latest`   | `.github/workflows/website.yml` |
| API (dev) | `ghcr.io/sandermvanvliet/roadcaptain-api:dev-latest`| `.github/workflows/build-api-dev.yml` |
| API (prod)| `ghcr.io/sandermvanvliet/roadcaptain-api:latest`    | `.github/workflows/build-api-prod.yml` |

Each workflow's `deploy` job SSHes into the server and runs `docker compose pull && docker compose up -d`
in the corresponding directory below. That means each directory must already exist on the server with its
`docker-compose.yml` in place (copy the matching file from this folder):

- `/opt/roadcaptain/site`
- `/opt/roadcaptain/api-dev`
- `/opt/roadcaptain/api-prod`

## Required GitHub secrets

- `DEPLOY_HOST` / `DEPLOY_USER` / `DEPLOY_SSH_KEY` — SSH access to the server, shared by all three workflows.

## GHCR package visibility

By default a newly published GHCR package is private. Either make each package public, or have the
server authenticate before pulling:

```bash
echo $GHCR_PAT | docker login ghcr.io -u sandermvanvliet --password-stdin
```

## API database file

`RoadCaptainDataContext` opens `database.sqlite3` relative to the working directory, so both API
compose files bind-mount a single file into the container at `/app/database.sqlite3`. Docker creates a
missing bind-mount source as a directory, not a file, which breaks SQLite — so before the first deploy,
create the file on the host:

```bash
touch /opt/roadcaptain/api-dev/database.sqlite3
touch /opt/roadcaptain/api-prod/database.sqlite3
```

## Site port / reverse proxy

The site was previously served by nginx directly from `/var/www/roadcaptain.nl`. It now runs as a
container publishing port 8081 on the host (see `site/docker-compose.yml`); update the host's nginx
config to reverse-proxy `roadcaptain.nl` to `127.0.0.1:8081` instead of serving static files.

The API ports (dev=6000, prod=5000) are unchanged, so the existing reverse-proxy config for
`api.roadcaptain.nl` keeps working as-is.
