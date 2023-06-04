# RoadCaptain infrastructure

The moving parts are:

1. The site: https://roadcaptain.nl
2. The API: https://api.roadcapain.nl

Both run on a single server using Nginx in these flavours:

1. The site: static file serving
2. The API: a reverse proxy and TLS terminator in front of a dotnet core Kestrel instance.
