# Changelog

## 0.6.1.0

### Route Builder

- Fix issue where loading a route for a different sport than the default sport fails

## 0.6.0.0

This is a big release with two items that will make RoadCaptain a lot more versatile!

With this release the RoadCaptain UI has been prepared to support multiple worlds! You'll no longer be stuck in Watopia but can also build routes in Makuri Islands 🎉🎉
All Zwift worlds are visible in Route Builder but for now only Watopia is selectable, the other worlds need to be added still.

As well as multiple world support, RoadCaptain now also supports running!
When you start the Route Builder you'll see the option to select a sport to build a route for. Especially for Watopia where there are dedicated run segments you can now include those on your routes!

Have a look at the new RoadCaptain Route Builder:

![Screenshot of Route Builder showing world and sport selection](./images/route-builder-multi-world-sport.png)

The RoadCaptain Runner now also displays the world and sport of a route when you select it:
![Screenshot of Runner with sport and world info of a selected route](./images/runner-route-running.png)

Of course the Runner will also tell you to either start cycling or running depending on the sport type of the route 😉

And there are a lot of other tweaks and fixes in this release:

### Route Builder

- **[Watopia]** Split the spwan point segment along the beach to for the new junction to Jons Route (running only)
  Routes that were created prior to this release will still work, RoadCaptain automatically corrects to the right spawn point
- **[Watopia]** Cleaned up the segments at the end of the beach so it looks a bit better
- **[Watopia]** Removed a segment from the cycling starting pens as that's only reachable from events
- **[Watopia]** Running segments are now included, they are only visible when you select running as the sport to build a route for
- **[Makuri Islands]** Added segments and spawn points, this is very much a work in progress still.
- **[User Experience]** When you select a sport for the first time RoadCaptain will ask you to use it as the default sport.

### Runner

- Sport type is now shown when you select a route
- The messages shown by the runner when waiting for Zwift to connect or an activity to start will now use the right term. For example: _Start running..._ / _Start cycling..._

## 0.5.6.0

- The runner and route build now use unicode arrow glyphs instead of images for the turn indicators because it looks a bit cleaner as the images don't scale very well.

### Route Builder

- When removing the last segment, the new last segment in the route list will be automatically selected.

## 0.5.5.0

- When starting, both the runner and route builder will now check if there is a new release available and shows a window to inform you if there is: ![Screenshot of a window showing the new version information](./images/runner-new-version.png) ([issue #15](https://github.com/sandermvanvliet/RoadCaptain/issues/15))

### Route Builder

- Dialogs should now appear in or over the main window instead of outside of it. ([issue #26](https://github.com/sandermvanvliet/RoadCaptain/issues/26))
- Fix an issue where opening a route after removing all segments of the current route would ask you to save the route first. ([issue #25](https://github.com/sandermvanvliet/RoadCaptain/issues/25))
- When adding the starting segment the name is now shown in the status bar instead of just "Added segment". ([issue #24](https://github.com/sandermvanvliet/RoadCaptain/issues/24))

## 0.5.4.0

### Runner

- Fixed the issue with RoadCaptain Runner not starting for some people. The installer now includes the missing files. ([issue #14](https://github.com/sandermvanvliet/RoadCaptain/issues/14))

### Route Builder

- Use segment name instead of id in the status bar messages to avoid confusion. ([issue #20](https://github.com/sandermvanvliet/RoadCaptain/issues/20))
- The _Open file_ and _Save file_ dialogs now have the initial location set to My Documents instead of the installation directory. ([issue #19](https://github.com/sandermvanvliet/RoadCaptain/issues/19))

## 0.5.3.0

- Desktop shortcuts for both the Runner and Route Builder will be created when installing RoadCaptain ([issue #5](https://github.com/sandermvanvliet/RoadCaptain/issues/5))
- Most segments now have a more descriptive name than `watopia-foo-bas-001-after-before-before` which should make things a lot easier to read. This is a _work in progress_ and may see changes later. ([issue #6](https://github.com/sandermvanvliet/RoadCaptain/issues/6))

### Runner

- Add better logging of errors during start up. ([issue #14](https://github.com/sandermvanvliet/RoadCaptain/issues/14))

### Route Builder

- Fixed bug #8 where you could not leave segment `watopia-four-horsemen-002-before` ([issue #8](https://github.com/sandermvanvliet/RoadCaptain/issues/8))
- Add better logging of errors during start up. ([issue #14](https://github.com/sandermvanvliet/RoadCaptain/issues/14))
- Route Builder will now show a message when you can't select a segment because it's unsupported: ![Screenshot showing error mesage in status bar](./images/route-builder-no-select-reason.png)
See: ([issue #3](https://github.com/sandermvanvliet/RoadCaptain/issues/3)) and ([issue #13](https://github.com/sandermvanvliet/RoadCaptain/issues/13))
- Added missing turn information for the loop at the start side of [Tempus Fugit](https://zwiftinsider.com/route/tempus-fugit/) on Fuego Flats. ([issue #13](https://github.com/sandermvanvliet/RoadCaptain/issues/13))
- You can now delete the last selected segment from the route by either pressing <kbd>Ctrl</kbd>+<kbd>Z</kbd> or selecting it in the list and pressing <kbd>Del</kbd> ([issue #4](https://github.com/sandermvanvliet/RoadCaptain/issues/4))

## 0.5.2.0

### Runner

- Fixed a bug where the in-game window would show only `Waiting for Zwift connection...` and the route name without giving any instructions when RoadCaptain is started but Zwift is not.
- When you've completed your route the in-game window now shows that a lot better: ![finish flag on last segment](./images/runner-finished-route.png) 
(when you're on the last segment of the route the finish flag is not yet visible 😉)
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