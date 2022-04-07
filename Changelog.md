# Changelog

## 0.5.0.0

### Route Builder

- When a segment is selected in the list it will be highlighted in green on the map
- Fix the direction of segment `watopia-bambino-fondo-001-after-after-before-before-before` which was in reverse

## 0.4.0.0

- Fix issue in building the installer

### Runner

- Reworked state machine to improve interaction with Zwift. This is mostly a "behind the scenes" change but it does improve the reliability of pairing with Zwift.

## 0.3.0.0

- Changed from .Net 5 to .Net 6 because version 5 is end-of-life per 8th of May 2022

### Runner

- Log files are now written to `%UserProfile%\AppData\Local\Codenizer BV\RoadCaptain`. This fixes an issue where log files wouldn't be written at all because RoadCaptain doesn't have write access to the installation directory (`Program Files (x86)\RoadCaptain`)
- RoadCaptain will now show an error message if it can't connect to the Zwift back-end to initiate the game pairing
- If the connection with Zwift is lost, RoadCaptain will show a message and re-initiates the game pairing

## 0.2.0.0

In this release you no longer will be asked for your username and password for Zwift, instead you'll be taken to the Zwift website to log in.

This means you won't have to worry that RoadCaptain sees our username and password for Zwift.