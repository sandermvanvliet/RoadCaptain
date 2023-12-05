---
title: "RoadCaptain Feature: Loops"
ParentFeatureList: "route-builder-features"
---

Loops are a special type of route that you can build in Route Builder which allow you to repeatedly ride the same section of a route. This can be particularly useful when you want to build a route with a specific training goal in mind.

## Creating a loop

In Route Builder you can select the segments to make your loop as you would normally build a route. When the last segment you've added links to a segment earlier on the route, Route Builder will ask you wether you want to create a loop:

![loop screenshot](/images/route-builder-loop.png)

When you select either _Infinite_ or _Loop_ and number, Route Builder converts your route into a loop. The color of the route on the map changes to indicate which part of the route is the actual loop (in yellow) and which parts are lead-in or lead-out (blue dashes):

![Screenshot of Route Builder with a looped route showing the colors](/images/route-builder-loop-map-colors.png)

## Editing a loop

When you've created a loop in your route, in the segment list on the left side you'll see an orange button at the segment that is the start of the loop. This button shows what type of loop this is:

- An _infinite_ loop. Meaning, you'll just keep going round and round
- A _constrained_ loop. Meaning, you'll do the loop **n** times and then continue with the rest of the route

You can click the button to bring up the _loop creation dialog_ where you can change between the loop types or even remove the loop:

![Screenshot of the loop edit dialog](/images/route-builder-loop-edit.png)

## Lead-in and lead-out

Loops don't have to start at the spawn point. Route Builder allows you to create a route where you ride from a spawn point to the segment where you want to start the loop.
When you add the last segment of the loop that connects to the route you are building, the first part of the route will be marked as a _lead-in_. In the segment overview the indicator for lead-in segments is a dashed line.

During a ride, Road Captain will show that you are on the lead-in of a loop at the bottom of the game window:

![in-game window lead in](/images/runner-loop-lead-in.png)

The loop counter will start incrementing as soon as you start the loop for the first time. Every next loop it will automatically increase so you can keep track of how many loops you've completed:

![in-game window loop count 2](/images/runner-loop-count-2.png)