# RoadCaptain infrastructure

The moving parts are:

1. The site: https://roadcaptain.nl
2. The API: https://api.roadcapain.nl

Both run on a single server as Docker containers (see [`docker/README.md`](docker/README.md)), fronted
by Nginx as a reverse proxy and TLS terminator:

1. The site: Nginx container serving the built Hugo output, reverse-proxied by the host Nginx.
2. The API: a dotnet core Kestrel instance in a container, reverse-proxied by the host Nginx.

`systemd/` documents the previous non-containerized deployment and is kept for reference / rollback.
