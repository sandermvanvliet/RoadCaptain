# Changelog

## 0.5.2.0

### Runner

- Fixed a bug where the in-game window would show only `Waiting for Zwift connection...` and the route name without giving any instructions when RoadCaptain is started but Zwift is not.
- When you've completed your route the in-game window now shows that a lot better: ![finish flag on last segment](./images/runner-finished-route.png) (when you're on the last segment of the route the finish flag is not yet visible 😉)
- Tweaked the in-game UI to use black text for the ascent/descent numbers instead of gray because it looks a bit better.

## 0.5.1.0

### Runner

- Fixed a bug where the elapsed ascent and descent would have a negative sign even though the numbers would increase.

## 0.5.0.0

### Runner

- Added a button to launch the Route Builder directly from the runner
- The version of RoadCaptain is now shown in the main window and has a link to the changelog
- When a route is selected the main window now displays the name, world and Zwift route: ![RoadCaptain main window showing route details](./images/changelog-route-details.png)

### Route Builder

- When a segment is selected in the list it will be highlighted in green on the map
- Fix the direction of segment `watopia-bambino-fondo-001-after-after-before-before-before` which was in reverse
- Routes can now be saved as GPX files so that they can be imported in other tools
- You can now set a custom name for the route, previously this was always set to the Zwift route name you would start on
- The version of RoadCaptain is now shown in the status bar and has a link to the changelog

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