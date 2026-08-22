# SystemD

> **Superseded:** deployment now happens via Docker images built and pushed by CI, run with
> `docker compose` on the server. See [`../docker/README.md`](../docker/README.md). These units are
> kept here for reference / rollback only.

As the `roadcaptain` user:

- Deploy these units to `~/.config/systemd/user`
- Run `systemctl enable --user RoadCaptainApiDev`
- Run `systemctl start --user RoadCaptainApiDev`

Ensure that the `roadcaptain` user can linger:

```bash
$> loginctl enable-linger roadcaptain
```
