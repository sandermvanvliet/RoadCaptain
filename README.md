# RoadCaptain

This is the development repository for RoadCaptain. The official site is at [roadcaptain.nl](https://roadcaptain.nl)

## What is RoadCaptain?

RoadCaptain is an app that makes riding on Zwift even more fun and can really push your limits in Watopia.

> Can't wait? Download RoadCaptain [right here](https://github.com/sandermvanvliet/RoadCaptain/releases/latest)

How? Simple: you are no longer limited to the fixed routes in Watopia, with RoadCaptain you can build your own routes and explore Watopia even more.

Always wanted to do 3 laps on the Volcano as a warm up followed by blasting through the Jungle Loop? Now you can!

Of course, you can already _sort of_ do this by starting a free ride in Zwift and using the turn buttons in the game but when you're powering through those segments it's super easy to miss the turn and that's not great for your flow right?

RoadCaptain takes away all the hassle of having to keep paying attention to upcoming turns and remembering which ones to take to follow the route you want.

So how does RoadCaptain make that work?

When you start RoadCaptain it will connect to Zwift and receive position updates and upcoming turns when you are riding. Your current position is matched against the route you've designed so RoadCaptain knows which turn to take next. When you are getting close to a turn (when you go past the turn marker on the side of the road), RoadCaptain tells Zwift which turn to take.

Sounds simple right? It also means you don't have to keep thinking about which way to go and can concentrate on pushing the power to the pedals!

For more information, head over to our official website at [roadcaptain.nl](https://roadcaptain.nl)

## Requirements and installing

RoadCaptain requires a Windows PC with .NET Desktop Runtime 6.0.3 installed which you can download [from here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.3-windows-x64-installer)).

I've tested RoadCaptain with Zwift running on the same machine. _However_, as long as you are on the same network you can use Zwift on an iPad or Apple TV as long as you have a PC nearby where you can start the RoadCaptain Runner app.

The RoadCaptain installer can be found [on the releases section](https://github.com/sandermvanvliet/RoadCaptain/releases/latest)

When you start the RoadCaptain Runner and click _Let's go!_ for the first time, Windows will ask you to allow network traffic on the private network. This is expected and is required for RoadCaptain to be able to talk to Zwift. If you accidentally click _deny_ you will need to uninstall and re-install RoadCaptain for this dialog to show again.

Before running Zwift I would recommend that you change Zwift to use windowed instead of full-screen mode. To change this go to the Zwift settings file (`prefs.xml`) (should be in your `My Documents\zwift` folder). Open that file and change `<FULLSCREEN>1</FULLSCREEN>` to `<FULLSCREEN>0</FULLSCREEN>`, save and close the file.

## Notes on testing

This is a beta version of RoadCaptain so expect quite a few rough edges and bugs. 

Some known issues:

- Currently you can't use Zwift Companion and RoadCaptain at the same time.
- The in-game screen most likely won't show up when running Zwift in full-screen mode. (see above in the installing section)
- RoadCaptain currently does not support iPad

If you want to report a bug or provide other feedback, feel free to send me an email at [info@roadcaptain.nl](mailto:info@roadcaptain.nl) or [create an issue](https://github.com/sandermvanvliet/RoadCaptain/issues).

RoadCaptain generates log files, you can find those in a directory under your user profile (typically `%userprofile%\AppData\Local\Codenizer BV\RoadCaptain`). Please send them along (or the latest one) when you report a bug. That will help me diagnose the problem a lot quicker 👍

## Last but not least

Please note that RoadCaptain or myself are not associated with Zwift and the app has been built purely as an interesting experiment to see if I could do it.

And yes, the screens do look a bit like Zwift but I hope I've made them just "off" enough to make it clear that it isn't Zwift itself.
