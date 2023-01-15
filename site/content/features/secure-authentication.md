---
title: "RoadCaptain Feature: Zwift Secure Authentication"
ParentFeatureList: "runner-features"
---

When RoadCaptain starts, it will connect to the Zwift platform and initiates the connection between Zwift and RoadCaptain. The Zwift platform requires an access token so that it can authenticate these kinds of requests and keep your account secure.

To obtain an access token, RoadCaptain starts a browser window that allows you to securely log in to Zwift without RoadCaptain seeing your username and password. Once you've logged in, Zwift provides this access token to RoadCaptain so that it can connect to the Zwift platform.

![The Zwift login window](/images/secure-auth-step-2.png)

With this approach, your username and password for Zwift are protected and RoadCaptain can't log in on your behalf. That keeps your account secure and prevents your account details from accidentally being leaked.

To ensure your account stays safe, RoadCaptain also will not store the access token. Once you close RoadCaptain, you will need to log in again the next time RoadCaptain starts. While this may be inconvenient it is by design to keep your account safe.