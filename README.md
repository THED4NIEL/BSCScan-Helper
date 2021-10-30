# BSCScan-Helper
This application runs in the system tray only.

Copy transaction hashes or addresses and use WIN+ESCAPE to open respective sites directly in the standard browser.
Alternatively you can be notified if the clipboard contains an address and open it by clicking on the notification.

![image](https://user-images.githubusercontent.com/86501450/139560031-636af421-b66c-485d-85d9-4e7746e46dc5.png)
_Example of notification_

### Additional options in config file:
- Set `UseInstantcopy` to `true` or `false` if you want to enable/disable automatic triggering of CTRL+C on selection
- Set `AllowNotifications` to `true` or `false` if you want to enable/disable automatic notifications
- change `Server` to use a different compatible service (like etherscan.io)
