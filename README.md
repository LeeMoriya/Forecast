# Forecast
Forecast adds a new dynamic weather system to the game with many configurable settings.

### How it works
Forecast detects exterior rooms based on the room's `DangerType` and the number of open tiles in the room's ceiling. A `WeatherController` object gets added to these rooms that controls the amount of rain fall and other weather effects. Weather settings for the current room and region are determined as follows:

1. The Weather Controller will first load the 'Global Settings' the user has configured in the Remix menu.

2. If the region has it's own custom weather settings then it will overwrite the Global Settings with it's own.

3. If the current room has it's own weather settings that differ from the Global settings, then these will be applied instead.

### Support Mode
By default, if a region has it's own weather settings defined, the Weather Controller will use these. However the user has the option to overwrite these custom settings with their own Global settings via the Remix menu.

The user can also enable 'Support Mode' where weather effects are disabled for all regions without custom settings. In this mode Forecast acts more as a dependency mod.

### Custom Settings
Region makers can define their own Global settings that apply to their region and tweak weather for each room individually if desired. For instructions on how to set this up, follow the guide below:

[Region Maker's Guide](https://github.com/LeeMoriya/Forecast/rain/RegionGuide.md)

### Download
Downloading Forecast from the Steam Workshop is recommended to ensure you stay up to date, however if you got the game from a different storefront, you can download it manually below:

[Download]()

Unzip the archive to `Rain World\RainWorld_Data\StreamingAssets\mods`
