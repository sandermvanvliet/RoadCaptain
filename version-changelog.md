
With this release a lot of work has been done behind the scenes to keep the app running smoothly and to remove some of the quirks that were added while RoadCaptain was initially built. The sad thing is you won't see most of that but it does mean that things like exiting from an activity is now handled a lot smoother as well as the initial connection with Zwift (where it says "Waiting for Zwift" for example).

Ultimately you _should_ notice less quirky behaviour at times which means you can keep focusing on riding instead of checking whether the app still works!

### Route Builder

- Show the segment id when hovering over the segment name in the route list

### Runner

- Fix an issue where RoadCaptain would flip between on/off route very quickly many times
- Automatically re-initialize the Zwift connection when the connection secret doesn't match
- Fix an issue where the in-game window would only show the Zwift route to start without any other instruction when clicking "Let's go" and Zwift is not yet started
- Removed the "Start new loop at end of route" as that is now handled by actual looped routes built with RouteBuilder

### Routing

- Fixed an incorrect turn from Volcano Climb to Volcano Circuit 1, this also introduces version 3 of the stored route files.
- Remove segment watopia-gran-fondo-002 because it's not routable

