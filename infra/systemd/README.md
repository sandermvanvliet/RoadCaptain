# SystemD

As the `roadcaptain` user:

- Deploy these units to `~/.config/systemd/user`
- Run `systemctl enable --user RoadCaptainApiDev`
- Run `systemctl start --user RoadCaptainApiDev`

Ensure that the `roadcaptain` user can linger:

```bash
$> loginctl enable-linger roadcaptain
```
