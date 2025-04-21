# Jellyfin-Notify
A work in progress Jellyfin plugin. For now I would like to use ntfy to notify users of new episodes for shows they have watched. Users will recieve a notification for each new episode of a show if they have watched one episode in the series. On the notification there is an unsubscribe button that should work on all platforms so the users can remove notifications for a series they no longer watch.

The ultimate goal is for this to be easily extensible to support a wide range of notification platforms and purposes. After NTFY functionality is done I would like to focus on creating a nice looking email that can be sent out weekly containing details on new episodes, their content, and when they will be released.

Currently in the proof of concept phase with a lot of refactoring effort needed once I complete some initial testing.

If you would like to try this out:

Add the plugin manifest.json to the plugin catalog: `https://raw.githubusercontent.com/jaredglaser/Jellyfin-Notify/refs/heads/main/manifest.json`

Find the plugin in the plugin catalog under `Notifications`, install it, and restart jellyfin.
