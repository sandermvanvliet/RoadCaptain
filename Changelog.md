# Changelog

## 0.6.4.0

### Runner

- Performance optimisation of game coordinate conversion. This is now roughly twice as fast which means that determining your position in-game happens much faster now and that means RoadCaptain can make routing decisions quicker!
- Fixed an issue where you had to log in again after exiting a Zwift activity

### Route Builder

- Fix an issue where an exception would occur when you load a route from the main screen of RouteBuilder. In this situation the segments aren't loaded yet which caused a crash to happen [#87](https://github.com/sandermvanvliet/RoadCaptain/issues/87)
- Clear highlighted segment when clearing the route [#88](https://github.com/sandermvanvliet/RoadCaptain/issues/88)
- Fix issue where updating the command executable flag would fail because it is not on the UI thread

### Routing

- Fix turn from Sequoia to Epic onto Ocean Boulevard 2 [#85](https://github.com/sandermvanvliet/RoadCaptain/issues/85)
- Fix turn from Ocean Boulevard 1 to Zwift KOM / Beach Road [#85](https://github.com/sandermvanvliet/RoadCaptain/issues/85)

## 0.6.3.1

I'm super happy to annouce that with version 0.6.3.1 RoadCaptain is now supported on macOS! 🎉🎉🎉

Over the last month and a bit the applications (RouteBuilder and Runner) have been ported to the [Avalonia UI](https://avaloniaui.net/) framework which makes it possible to support multiple platforms with a single codebase.
That also paves the way for a Linux version in the future.

For now the macOS build is based on the pre-M1 architecture which means that RoadCaptain will run using Rosetta and not yet natively on ARM devices. This has to do with the webview component that is used to log in to Zwift, there are still some issues with packaging that need to be resolved before that becomes available.

Another big change is that it is no longer required to install the .Net Core runtime. Both the Windows and macOS builds are now stand alone applications which means that installation is much simpler now.

**Known issues:**

A number of routing issues were found (see [#84](https://github.com/sandermvanvliet/RoadCaptain/issues/84), [#82](https://github.com/sandermvanvliet/RoadCaptain/issues/82), [#77](https://github.com/sandermvanvliet/RoadCaptain/issues/77), [#75](https://github.com/sandermvanvliet/RoadCaptain/issues/75), [#73](https://github.com/sandermvanvliet/RoadCaptain/issues/73) and [#72](https://github.com/sandermvanvliet/RoadCaptain/issues/72)). These are still under investigation and should be addressed in the next build.

### Route Builder

- Buttons no longer drop off the screen when resizing the window. [#55](https://github.com/sandermvanvliet/RoadCaptain/issues/55)
- Tooltips are now visible on buttons. [#40](https://github.com/sandermvanvliet/RoadCaptain/issues/40)
- When saving a route and the first and last segments are connected, Route Builder will ask you whether to make the route a loop. [#59](https://github.com/sandermvanvliet/RoadCaptain/issues/)
- The map now no longer drops off the side of the screen. [#55](https://github.com/sandermvanvliet/RoadCaptain/issues/55)
- The map looks a bit nicer now, anti-aliasing improves the smoothness of the segments.
- Tooltips are now shown on KOMs and Sprints so that you can see which ones they are.

### Runner

- **[PREVIEW]** Automatically end the Zwift activity after completing the route [#58](https://github.com/sandermvanvliet/RoadCaptain/issues/58)
- **[PREVIEW]** Start another loop of the route if the route is a loop [#59](https://github.com/sandermvanvliet/RoadCaptain/issues/59). When you select this option, the "end activity" option is automatically disabled.
- User preferences have been simplified, on Windows there is only 1 directory left instead of 2. [#74](https://github.com/sandermvanvliet/RoadCaptain/issues/74)
- When you end the activity in Zwift, the Runner will automatically return to the start screen. [#67](https://github.com/sandermvanvliet/RoadCaptain/issues/67)
- Show a message when route lock is lost, now you'll see when RoadCaptain loses track in-game. [#66](https://github.com/sandermvanvliet/RoadCaptain/issues/66)
- The new version popup now shows properly formatted content instead of raw Markdown. [#61](https://github.com/sandermvanvliet/RoadCaptain/issues/61)
- When selecting a route the details now also show distance, ascent and descent. [#51](https://github.com/sandermvanvliet/RoadCaptain/issues/51)
- A message box is shown when another instance of RoadCaptain Runner is already running.

The **[PREVIEW]** items should work but may have some quirks. Any feedback is more than welcome!
## 0.6.2.1

**Breaking change:**

Due to changes in the turns for Watopia (see [issue #62](https://github.com/sandermvanvliet/RoadCaptain/issues/62)), routes that traversed the Jungle Loop Switchback segment won't work properly anymore. You will have to rebuild the route to make those routes work again.

### Route Builder

- Migrate settings from previous installed version of RoadCaptain so that settings are preserved ([issue #60](https://github.com/sandermvanvliet/RoadCaptain/issues/60))
- Reset route name when clearing the route, opening a route or navigating back to the world selection screen ([issue #54](https://github.com/sandermvanvliet/RoadCaptain/issues/54))

### Runner

- Migrate settings from previous installed version of RoadCaptain so that settings are preserved ([issue #60](https://github.com/sandermvanvliet/RoadCaptain/issues/60))
- Fix an issue where progress would not be tracked on the last segment of the route ([issue #64](https://github.com/sandermvanvliet/RoadCaptain/issues/64))
- Fix an issue where the Let's Go! button would remain grey after logging in and selecting a Rebel Route ([issue #65](https://github.com/sandermvanvliet/RoadCaptain/issues/65))
- Handle 3-way junctions properly when only left/right turn commands are received from Zwift ([issue #57](https://github.com/sandermvanvliet/RoadCaptain/issues/57))

### Routing

- [Watopia] Fix the turn at the end of the Jungle Loop Switchback towards the Jungle Loop Rope Bridge segment ([issue #62](https://github.com/sandermvanvliet/RoadCaptain/issues/62))

## 0.6.2.0

### Route Builder

- Add button to remove last segment ([issue #36](https://github.com/sandermvanvliet/RoadCaptain/issues/36))
- Enable buttons only when they would have an effect (disables reset, save, simulate when on the world selection view) ([issue #39](https://github.com/sandermvanvliet/RoadCaptain/issues/39))
- Fix a visual bug where the start/end markers of the route would be reversed if the spawn point segment has a reverse direction.
- Add spawn point just before the Fuego Flats finish arch which is from the Triple Flat Loops route ([issue #38](https://github.com/sandermvanvliet/RoadCaptain/issues/38))
- Add spawn point in the volcano which is from the Tour of Fire and Ice route ([issue #38](https://github.com/sandermvanvliet/RoadCaptain/issues/38))
- Open and save file dialogs now re-open at the last used location instead of always defaulting to My Documents ([issue #27](https://github.com/sandermvanvliet/RoadCaptain/issues/27))
- The map now supports mouse-wheel / pinch zoom ([issue #44](https://github.com/sandermvanvliet/RoadCaptain/issues/44))

### Runner

- Open file dialog now re-opens at the last used location instead of always defaulting to My Documents ([issue #27](https://github.com/sandermvanvliet/RoadCaptain/issues/27))
- You can now select the Rebel Routes from Zwift Insider as pre-built routes ([issue #28](https://github.com/sandermvanvliet/RoadCaptain/issues/28)):
![Screenshot of runner with Rebel Routes dropdown](./images/runner-rebel-routes.png)
- Preent multiple instances of RoadCaptain from starting ([issue #53](https://github.com/sandermvanvliet/RoadCaptain/issues/53)):

### Routing

- [Watopia] Fix the turn at the end of the Volcano to Villas segment ([issue #41](https://github.com/sandermvanvliet/RoadCaptain/issues/41))
- [Watopia] Add right-turn from Italian Villas loop 4 to Italian Villas loop 3 ([issue #37](https://github.com/sandermvanvliet/RoadCaptain/issues/37))

## 0.6.1.0

### Route Builder

- Fix issue where loading a route for a different sport than the default sport fails
- Show route start and end markers on the map ([issue #31](https://github.com/sandermvanvliet/RoadCaptain/issues/31))
- Show KOM start (red), finish (green) and segment (orange) on the map ([issue #31](https://github.com/sandermvanvliet/RoadCaptain/issues/31)):
![Screenshot of Route Builder map with KOM markers shown in green and red](./images/route-builder-koms.png)
- Show sprint start (red), finish (green) and segment (purple) on the map ([issue #31](https://github.com/sandermvanvliet/RoadCaptain/issues/31)):
![Screenshot of Route Builder map with sprints markers shown in green and red](./images/route-builder-sprints.png)
- Fixed a layout issue where the buttons would overlap the map if the window is resized to a relatively small size.
- Show a warning when you select a segment that can't be reached from the spawn point ([issue #33](https://github.com/sandermvanvliet/RoadCaptain/issues/33))
- Fix issue where the route path direction would be incorrect for the starting segment when the direction is reversed. This would show the start marker on the wrong end of the starting segment ([issue #33](https://github.com/sandermvanvliet/RoadCaptain/issues/33))
- It is now possible to zoom and pan the map view ([issue #29](https://github.com/sandermvanvliet/RoadCaptain/issues/29)):
![Screen recording of panning and zooming the map in Route Builder](./images/route-builder-panzoom.gif)

### Runner

- Fix issue where the Runner would show `RoadCaptain.World` instead of `Watopia` as the world name after loading a route.
- Fix issue where the Runner would show `RoadCaptain.World` instead of `Watopia` as the world name in the in-game window.

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