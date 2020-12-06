# Events Pack

### Author: Crawcik
Powered by: Smod2

![](https://img.shields.io/github/release/Crawcik/SL-EventPack) ![](https://img.shields.io/github/license/Crawcik/SL-EventPack)

------------

##### Build-in events list:
- **Versus** - Everyone has a gun, D-Class and Scientists fights in LCZ
- **Dziady** - Only SCP is 049, GATE A is opened, sometimes zombies are spawning
- **Blackout** - The lights go off and you have a flashlight running low
- **Saxton Hale** - Boss fight with superpowers

##### How to download it?
1. Download lastest version from: https://github.com/Crawcik/SL-EventPack/releases/
2. Move "EventsManager.dll" to ***GameFolder***/sm-plugins/
3. Run server once (Plugin will create folder)
4. Put Gamemodes/Events to ***GameFolder***/sm-plugins/Events/

##### How do I get Gamemodes/Events
- You can get default plugins that I created: https://github.com/Crawcik/SL-EventPack/releases/ (from EventsPack.zip)
- You can get some from community (if someone made)
- You can create your own. Use "EventsManager.dll" as reference in CSPROJ, and add to your class "EventManager.GameEvents" class as *base class*

##### How to change translation?
- Open ***GameFolder***/sm-plugins/Events/translation.json and change text

##### I dont have Smod2...
1. Download lastest "Smod2 FILES" from: https://github.com/ServerMod/Smod2/releases/
2. Unzip folder and move files to ***GameFolder***/SCPSL_Data/Managment/
3. Run server for first configuration
4. Folder ***GameFolder***/sm-plugins/ will be automatically created
