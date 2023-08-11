# Region Maker's Guide
You can use Forecast to set up specific weather settings for your region via 'Tags' defined in a text file. Each configurable setting has an associated 'Tag' that you write in the text file, which is then read by Forecast. The full list of available 'Tags' and explanations for each are below.

## Setup
To start, in your region's World folder: `mods\your_mod\world\xx` create a new text file called `xx_forecast` where `xx` is your region's acronym.

Inside the text file you will first define the Global settings that will apply throughout your region. If you later define settings for individual rooms, they will overwrite the Global settings. e.g. you can disable Lightning Strikes in your region and then enable them for specific rooms.

Here's an example of the layout and formatting your text file should have:
```
GLOBAL: WC_100, WI_50, PL_90, WD_RIGHT, LS_OFF, ST_STUN, BG_ON, WA_ON, CC_ON, BL_ON, RV_ON

ROOMS
XX_A01 : WI_70, PL_60, LS_ON, LC_10_10, LC_255_255_240
XX_A02 : PL_80, LS_ON, LC_20_60, LC_255_255_240
XX_B01 : WD_MID, PL_40, WI_100, RV_OFF
END ROOMS
```

---
***WEATHER INTENSITY***
- Info: Controls whether rain intensity should increase as the cycle progresses or be a fixed value from 0-100.
- Examples: `WI_DYNAMIC`, `WI_20`, `WI_75`
- Notes: In dynamic mode the cycle has a chance of starting at higher intensities or might not start until mid-way through the cycle.
---
***WEATHER CHANCE***
- Info: Controls the chance that weather will occur this cycle or not based on a percentage from 0-100.
- Examples: `CH_100`, `CH_50`, `CH_0`
- Notes: Can only be used as a GLOBAL setting.
---
***PARTICLE LIMIT***
- Info: Controls the amount of particles that can appear in the room. This is the most performance heavy setting so it's not recommended to go above the default limit of 100.
- Examples: `PL_100`, `PL_70`, `PL_30`
- Notes: The value influences the particle limit and is not an exact number, e.g. `PL_100` does not mean only 100 particles can appear.
---
***WIND DIRECTION***
- Info: Controls the direction particles fall -- more noticeable at higher intensities.
- Examples: `WD_RANDOM`, `WD_LEFT`, `WD_MID`, `WD_RIGHT`
- Notes: If set to random, one of the three directions is chosen and used for that cycle.
---
***BACKGROUND COLLISION***
- Info: Controls whether rain particles can collide with elements in the background.
- Examples: `BG_OFF`, `BG_ON`
---
***WATER COLLISION***
- Info: Controls whether particles that hit water surfaces create ripples.
- Examples: `WA_OFF`, `WA_ON`
---
***BACKGROUND LIGHTNING***
- Info: Controls whether lightning flashes appear in the background at higher intensities.
- Examples: `BL_OFF`, `BL_ON`
- Notes: Will only occur when the intensity is above 70%.
---
***CLOUD COVER***
- Info: Controls whether the cloud cover in rooms should increase with intensity or be a fixed value.
- Examples: `CC_OFF`, `CC_ON`, `CC_80`
---
***RAIN VOLUME***
- Info: Rain sounds are automatically added and increase in volume with intensity, you can disable it here if you want to manually place sound sources.
- Examples: `RV_OFF`, `RV_ON`
---
***LIGHTNING STRIKES***
- Info: Controls whether lightning strikes can occur.
- Examples: `LS_OFF`, `LS_ON`
---
***LIGHTNING CHANCE***
- Info: Configure an interval in seconds that Forecast should attempt to create a lightning strike and the percentage chance that it will.
- Examples: `LS_10_50`, `LS_60_100`, `LS_5_15`
- Notes: The first number are seconds and the second is a percentage. E.g. `LS_10_50` is a 50% chance of a lightning strike occuring every 10 seconds.
---
***LIGHTNING COLOR***
- Info: Configure a custom color for the lightning strikes using RGB values.
- Examples: `LC_255_255_240`, `LC_0_255_0`
- Notes: Values are in RGB and range from 0-255.
---
***LIGHTNING STRIKE TYPE***
- Info: Controls what kind of damage a direct impact from a lightning strike will have, if any.
- Examples: `ST_NONE`, `ST_STUN`, `ST_LETHAL`
---
