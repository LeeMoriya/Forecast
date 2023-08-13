using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using System.IO;
using System.Reflection;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine.Video;

public class ForecastConfig : OptionInterface
{
    //Configurables
    public static Configurable<bool> supportMode;
    public static Configurable<int> displayMode;
    public static Configurable<int> weatherType;

    public static Configurable<int> weatherIntensity;
    public static Configurable<int> weatherChance;
    public static Configurable<int> windDirection;

    public static Configurable<int> particleLimit;

    public static Configurable<bool> backgroundCollision;
    public static Configurable<bool> waterCollision;
    public static Configurable<bool> dynamicClouds;
    public static Configurable<float> cloudCover;

    public static Configurable<bool> rainVolume;

    public static Configurable<bool> backgroundLightning;
    public static Configurable<bool> lightningStrikes;
    public static Configurable<int> lightningInterval;
    public static Configurable<int> lightningChance;
    public static Configurable<int> strikeDamageType;
    public static Configurable<Color> strikeColor;

    public static Configurable<bool> endBlizzard;
    public static Configurable<bool> effectColors;
    public static Configurable<bool> snowPuffs;

    public static Configurable<bool> debugMode;

    //Manual Configurables
    public static Dictionary<string, int> regionSettings;
    public static Dictionary<string, Dictionary<string, List<string>>> customRegionSettings;

    //Menu
    public OpImage rainBanner;
    public OpRect supportRect;
    public OpSimpleButton supportModeButton;
    public bool init = false;

    public UIelement[] settings;
    public OpSimpleButton intensityToggle;
    public OpSimpleButton windToggle;
    public OpScrollBox settingsBox;
    public OpSimpleButton bgToggle;
    public OpSimpleButton strikeToggle;
    public OpLabel supportWarning;
    public OpLabel supportWarningDesc;
    public OpSimpleButton strikeTypeToggle;
    public OpSlider intervalSlider;
    public OpSlider strikeChanceSlider;
    public OpSimpleButton backgroundCollisionToggle;
    public OpSimpleButton waterCollisionToggle;

    //Debug
    public OpSimpleButton debugButton;

    //Region
    List<OpRect> regionRects;
    List<OpLabel> regionLabels;
    List<OpSimpleButton> regionButtons;
    List<OpLabel> customLabels;

    public ForecastConfig(ForecastMod mod)
    {
        regionSettings = new Dictionary<string, int>();
        customRegionSettings = new Dictionary<string, Dictionary<string, List<string>>>();
        LoadCustomRegionSettings();

        weatherType = config.Bind<int>("weatherType", 0);
        supportMode = config.Bind<bool>("supportMode", false);

        weatherIntensity = config.Bind<int>("weatherIntensity", 0, new ConfigAcceptableRange<int>(0, 3));
        weatherChance = config.Bind<int>("weatherChance", 100, new ConfigAcceptableRange<int>(0, 100));
        windDirection = config.Bind<int>("windDirection", 0, new ConfigAcceptableRange<int>(0, 3));

        particleLimit = config.Bind<int>("particleLimit", 100);

        backgroundCollision = config.Bind<bool>("backgroundCollision", true);
        waterCollision = config.Bind<bool>("waterCollision", true);
        dynamicClouds = config.Bind<bool>("dynamicClouds", true);
        cloudCover = config.Bind<float>("cloudCover", 0.5f);

        rainVolume = config.Bind<bool>("rainVolume", true);

        backgroundLightning = config.Bind<bool>("backgroundLightning", true);
        lightningStrikes = config.Bind<bool>("lightningStrikes", true);
        lightningInterval = config.Bind<int>("lightningInterval", 10, new ConfigAcceptableRange<int>(1, 60));
        lightningChance = config.Bind<int>("lightningChance", 15);
        strikeDamageType = config.Bind<int>("strikeDamageType", 0);
        strikeColor = config.Bind<Color>("strikeColor", new Color(1f, 1f, 0.95f, 1f));

        endBlizzard = config.Bind<bool>("endBlizzard", true);
        effectColors = config.Bind<bool>("effectColors", true);
        snowPuffs = config.Bind<bool>("snowPuffs", true);

        debugMode = config.Bind<bool>("debugMode", false);
    }

    public static void LoadRegionWeather()
    {
        string savePath = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}ModConfigs{Path.DirectorySeparatorChar}Forecast";
        string filePath = $"{savePath}{Path.DirectorySeparatorChar}settings.txt";
        if (File.Exists(filePath))
        {
            try
            {
                regionSettings = new Dictionary<string, int>();
                string[] data = File.ReadAllLines(filePath);
                for (int i = 0; i < data.Length; i++)
                {
                    string[] regionWeather = data[i].Split(':');
                    if (!int.TryParse(regionWeather[1], out int weather))
                    {
                        weather = 1;
                    };
                    regionSettings.Add(regionWeather[0], weather);
                }
                ForecastLog.Log("Loaded region weather preferences");
            }
            catch (Exception ex)
            {
                File.Delete(filePath);
                Debug.LogException(ex);
                ForecastLog.Log("ERROR: There was an issue loading region weather preferences, resetting data...");
            }
        }
    }

    public static void SaveRegionWeather()
    {
        string savePath = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}ModConfigs{Path.DirectorySeparatorChar}Forecast";
        string filePath = $"{savePath}{Path.DirectorySeparatorChar}settings.txt";
        string data = "";
        //Create save folder if not present
        if(!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        foreach(KeyValuePair<string,int> pair in regionSettings)
        {
            data += $"{pair.Key}:{pair.Value}\n";
        }
        data.TrimEnd('\n');
        File.WriteAllText(filePath, data);
        ForecastLog.Log("Saved region weather preferences");
    }


    public static void LoadCustomRegionSettings()
    {
        string[] array = new string[]
        {
            ""
        };
        //Load list of installed regions
        string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
        if (File.Exists(path))
        {
            array = File.ReadAllLines(path);
        }

        LoadRegionWeather();

        for (int i = 0; i < array.Length; i++)
        {
            //If not already present, assign default values
            if (!regionSettings.ContainsKey(array[i]))
            {
                //TODO - method for default settings for vanilla regions
                regionSettings.Add(array[i], 1);
            }
            //Check if the region has custom weather settings
            if (File.Exists(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + array[i] + Path.DirectorySeparatorChar + array[i] + "_forecast.txt")))
            {
                ForecastLog.Log($"FORECAST: Custom settings found for {array[i]}");

                string[] data = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + array[i] + Path.DirectorySeparatorChar + array[i] + "_forecast.txt"));
                if (!customRegionSettings.ContainsKey(array[i]))
                {
                    customRegionSettings.Add(array[i], new Dictionary<string, List<string>>());
                    if (!regionSettings.ContainsKey(array[i]))
                    {
                        regionSettings[array[i]] = 2;
                    }
                }
                bool globalSettings = false;
                bool roomSection = false;
                for (int s = 0; s < data.Length; s++)
                {
                    //Load global settings for the region
                    if (data[s].StartsWith("GLOBAL:"))
                    {
                        globalSettings = true;
                        customRegionSettings[array[i]].Add("GLOBAL", new List<string>());

                        string[] globalTags = data[s].Split(':')[1].Split(',');
                        for (int g = 0; g < globalTags.Length; g++)
                        {
                            customRegionSettings[array[i]]["GLOBAL"].Add(globalTags[g].Trim());
                        }
                    }
                    if (data[s].StartsWith("END ROOMS"))
                    {
                        roomSection = false;
                    }
                    //Load room specific weather settings
                    if (roomSection)
                    {
                        string room = data[s].Split(':')[0].Trim();
                        if (!customRegionSettings[array[i]].ContainsKey(room))
                        {
                            customRegionSettings[array[i]].Add(room, new List<string>());
                        }
                        else
                        {
                            Debug.LogException(new Exception("FORECAST: Duplicate room name on line " + s));
                        }
                        string[] tags = data[s].Split(':')[1].Split(',');
                        for (int t = 0; t < tags.Length; t++)
                        {
                            customRegionSettings[array[i]][room].Add(tags[t].Trim());
                        }
                    }
                    if (data[s].StartsWith("ROOMS"))
                    {
                        roomSection = true;
                    }

                }
                if (!globalSettings)
                {
                    Debug.LogException(new Exception($"FORECAST: Custom settings for {array[i]} is missing GLOBAL settings!"));
                }
            }
        }

        //Debug - print loaded tags
        foreach (string reg in customRegionSettings.Keys)
        {
            foreach (string key in customRegionSettings[reg].Keys)
            {
                string tags = "";
                foreach (string tag in customRegionSettings[reg][key])
                {
                    tags += tag;
                    tags += " ";
                }
                ForecastLog.Log("FORECAST: " + key + ": " + tags);
            }
        }
    }

    public override void Initialize()
    {
        init = false;
        var options = new OpTab(this, "Options");
        var regions = new OpTab(this, "Regions");
        Tabs = new[]
        {
            options, regions
        };

        #region Options Tab

        //Rain and snow logos
        byte[] bytes = File.ReadAllBytes(AssetManager.ResolveFilePath("sprites\\rainLogo.png"));
        Texture2D texture = new Texture2D(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(bytes);
        rainBanner = new OpImage(new Vector2(300f, 540f), texture);
        rainBanner.anchor = new Vector2(0.5f, 0f);
        rainBanner.alpha = weatherType.Value == 1 ? 0f : 1f;

        //Version label
        OpLabel version = new OpLabel(300f, 525f, $"Version: 1.03     -     By LeeMoriya", false);
        version.color = new Color(0.4f, 0.4f, 0.4f);
        version.label.alignment = FLabelAlignment.Center;
        options.AddItems(version, rainBanner);

        //Support Mode
        supportRect = new OpRect(new Vector2(10f, 420f), new Vector2(580f, 80f));
        supportRect.colorEdge = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
        supportRect.colorFill = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0f, 0f, 0f);
        OpLabel supportTitle = new OpLabel(165f, 470f, "SUPPORT MODE");
        OpLabel supportDesc = new OpLabel(165f, 440f, "When support mode is active, Forecast will not generate any weather\nunless a region has custom settings defined.");

        supportModeButton = new OpSimpleButton(new Vector2(33f, 437f), new Vector2(110f, 45f), supportMode.Value ? "ENABLED" : "DISABLED");
        supportModeButton.OnClick += SupportModeButton_OnClick;
        options.AddItems(supportRect, supportTitle, supportDesc, supportModeButton);

        float settingsHeight = 1150f;
        settingsBox = new OpScrollBox(new Vector2(0f, 0f), new Vector2(600f, 400f), settingsHeight, false, true, true);
        options.AddItems(settingsBox);

        OpLabel globalSettings = new OpLabel(new Vector2(290f, settingsHeight - 35f), new Vector2(), "GLOBAL SETTINGS", FLabelAlignment.Center, true);
        OpLabel globalDesc = new OpLabel(new Vector2(290f, settingsHeight - 70f), new Vector2(), "Define settings that apply to all rooms and regions where weather is enabled.\nYou can configure which regions receive weather in the 'Regions' tab.", FLabelAlignment.Center);

        //BASIC SETTINGS
        float basicAnchor = settingsHeight - 140f;
        OpLabel basicLabel = new OpLabel(new Vector2(290f, basicAnchor + 15f), new Vector2(), "- BASIC SETTINGS -", FLabelAlignment.Center);

        OpRect basicSettingsRect = new OpRect(new Vector2(15f, basicAnchor - 313.5f), new Vector2(555f, 320f));
        basicSettingsRect.colorFill = new Color(0f, 0f, 1f);
        settingsBox.AddItems(basicSettingsRect, basicLabel);

        //Weather Intensity
        OpLabel intensityLabel = new OpLabel(160f, basicAnchor - 30f, "WEATHER INTENSITY");
        OpLabel intensityDesc = new OpLabel(160f, basicAnchor - 50f, "Adjust whether intensity should change over time or be a fixed value");
        intensityToggle = new OpSimpleButton(new Vector2(30f, basicAnchor - 55f), new Vector2(110f, 45f), IntensityValue());
        intensityToggle.OnClick += IntensityToggle_OnClick;
        settingsBox.AddItems(globalSettings, globalDesc, intensityLabel, intensityDesc, intensityToggle);

        //Weather Chance
        OpSlider weatherChanceSlider = new OpSlider(weatherChance, new Vector2(30f, basicAnchor - 120f), 110, false);
        OpLabel chanceLabel = new OpLabel(160f, basicAnchor - 110f, "WEATHER CHANCE");
        OpLabel chanceDesc = new OpLabel(160f, basicAnchor - 130f, "The chance that weather will occur each cycle");
        settingsBox.AddItems(weatherChanceSlider, chanceLabel, chanceDesc);

        //Particle Limit
        OpSlider particleLimitSlider = new OpSlider(particleLimit, new Vector2(30f, basicAnchor - 200f), 110, false);
        OpLabel particleLimitLabel = new OpLabel(160f, basicAnchor - 190f, "PARTICLE LIMIT");
        OpLabel particleLimitDesc = new OpLabel(160f, basicAnchor - 210f, "Influences the number of particles that can appear");
        settingsBox.AddItems(particleLimitSlider, particleLimitLabel, particleLimitDesc);

        //Wind Direction
        OpLabel windLabel = new OpLabel(160f, basicAnchor - 270f, "WIND DIRECTION");
        OpLabel windDesc = new OpLabel(160f, basicAnchor - 290f, "The direction particles will fall each cycle");
        windToggle = new OpSimpleButton(new Vector2(30f, basicAnchor - 295f), new Vector2(110f, 45f), WindDirectionValue());
        windToggle.OnClick += WindToggle_OnClick;
        settingsBox.AddItems(windLabel, windDesc, windToggle);

        //VISUAL SETTINGS
        float visualAnchor = settingsHeight - 510f;
        OpLabel visualLabel = new OpLabel(new Vector2(290f, visualAnchor + 15f), new Vector2(), "- VISUAL SETTINGS -", FLabelAlignment.Center);

        OpRect visualRect = new OpRect(new Vector2(15f, visualAnchor - 233.5f), new Vector2(555, 240f));
        visualRect.colorFill = new Color(1f, 0f, 1f);
        settingsBox.AddItems(visualRect, visualLabel);

        //Background Lightning
        OpLabel bgLabel = new OpLabel(160f, visualAnchor - 30f, "BACKGROUND LIGHTNING");
        OpLabel bgDesc = new OpLabel(160f, visualAnchor - 50f, "When weather intensity is high, lightning flashes can occur");
        bgToggle = new OpSimpleButton(new Vector2(30f, visualAnchor - 55f), new Vector2(110f, 45f), backgroundLightning.Value ? "ENABLED" : "DISABLED");
        bgToggle.OnClick += BgToggle_OnClick;
        settingsBox.AddItems(bgLabel, bgDesc, bgToggle);

        //Background Collision
        OpLabel backgroundCollisionLabel = new OpLabel(160f, visualAnchor - 110f, "BACKGROUND COLLISION");
        OpLabel backgroundCollisionDesc = new OpLabel(160f, visualAnchor - 130f, "Whether particles can collide with background elements");
        backgroundCollisionToggle = new OpSimpleButton(new Vector2(30f, visualAnchor - 135f), new Vector2(110f, 45f), backgroundCollision.Value ? "ENABLED" : "DISABLED");
        backgroundCollisionToggle.OnClick += BackgroundCollisionToggle_OnClick;
        settingsBox.AddItems(backgroundCollisionLabel, backgroundCollisionDesc, backgroundCollisionToggle);

        //Water Collision
        OpLabel waterCollisionLabel = new OpLabel(160f, visualAnchor - 190f, "WATER COLLISION");
        OpLabel waterCollisionDesc = new OpLabel(160f, visualAnchor - 210f, "Whether particles create ripples on water surfaces");
        waterCollisionToggle = new OpSimpleButton(new Vector2(30f, visualAnchor - 215f), new Vector2(110f, 45f), waterCollision.Value ? "ENABLED" : "DISABLED");
        waterCollisionToggle.OnClick += WaterCollisionToggle_OnClick;
        settingsBox.AddItems(waterCollisionLabel, waterCollisionDesc, waterCollisionToggle);


        //LIGHTNING SETTINGS
        float lightningAnchor = settingsHeight - 800f;
        OpLabel lightningLabel = new OpLabel(new Vector2(290f, lightningAnchor + 15f), new Vector2(), "- LIGHTNING SETTINGS -", FLabelAlignment.Center);

        OpRect lightningSettingsRect = new OpRect(new Vector2(15f, lightningAnchor - 313.5f), new Vector2(555f, 320f));
        lightningSettingsRect.colorFill = new Color(1f, 1f, 0f);

        settingsBox.AddItems(lightningLabel, lightningSettingsRect);

        //Lightning Strikes
        OpLabel strikeLabel = new OpLabel(160f, lightningAnchor - 30f, "LIGHTNING STRIKES");
        OpLabel strikeDesc = new OpLabel(160f, lightningAnchor - 50f, "Lightning strikes can occur at high intensity");
        strikeToggle = new OpSimpleButton(new Vector2(30f, lightningAnchor - 55f), new Vector2(110f, 45f), lightningStrikes.Value ? "ENABLED" : "DISABLED");
        strikeToggle.OnClick += StrikeToggle_OnClick;
        settingsBox.AddItems(strikeLabel, strikeDesc, strikeToggle);

        //Strike Interval
        intervalSlider = new OpSlider(lightningInterval, new Vector2(30f, lightningAnchor - 120f), 110, false);
        OpLabel intervalLabel = new OpLabel(160f, lightningAnchor - 110f, "LIGHTNING INTERVAL");
        OpLabel intervalDesc = new OpLabel(160f, lightningAnchor - 130f, "The minimum interval at which lightning can strike in seconds");
        settingsBox.AddItems(intervalSlider, intervalLabel, intervalDesc);

        //Strike Chance
        strikeChanceSlider = new OpSlider(lightningChance, new Vector2(30f, lightningAnchor - 200f), 110, false);
        OpLabel strikeChanceLabel = new OpLabel(160f, lightningAnchor - 190f, "LIGHTNING CHANCE");
        OpLabel strikeChanceDesc = new OpLabel(160f, lightningAnchor - 210f, "The percentage chance a strike will occur at each interval");
        settingsBox.AddItems(strikeChanceSlider, strikeChanceLabel, strikeChanceDesc);

        OpLabel strikeTypeLabel = new OpLabel(160f, lightningAnchor - 270f, "DAMAGE TYPE");
        OpLabel strikeTypeDesc = new OpLabel(160f, lightningAnchor - 290f, "What type of damage a lightning strike will inflict upon hit");
        strikeTypeToggle = new OpSimpleButton(new Vector2(30f, lightningAnchor - 295f), new Vector2(110f, 45f), StrikeDamageValue());
        strikeTypeToggle.OnClick += StrikeTypeToggle_OnClick;
        settingsBox.AddItems(strikeTypeLabel, strikeTypeDesc, strikeTypeToggle);

        //Support Label
        supportWarning = new OpLabel(new Vector2(290f, 220f), new Vector2(), "SUPPORT MODE ENABLED", FLabelAlignment.Center, true);
        supportWarningDesc = new OpLabel(new Vector2(290f, 180f), new Vector2(), "Forecast will only generate weather for regions with their own custom settings.\nTo allow weather for all regions, and to change the global settings, disable support mode.", FLabelAlignment.Center);
        options.AddItems(supportWarning, supportWarningDesc);

        #endregion

        #region Regions Tab

        regionRects = new List<OpRect>();
        regionLabels = new List<OpLabel>();
        regionButtons = new List<OpSimpleButton>();
        customLabels = new List<OpLabel>();

        string[] array = new string[]
        {
            ""
        };
        string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
        if (File.Exists(path))
        {
            array = File.ReadAllLines(path);
        }

        float scrollHeight = 70f * array.Length + 180f;
        float itemHeight = scrollHeight - 125f;
        OpScrollBox scrollBox = new OpScrollBox(regions, scrollHeight, false, true);
        regions.AddItems(scrollBox);

        debugButton = new OpSimpleButton(new Vector2(290f - 40f, 15f), new Vector2(80f, 30f), debugMode.Value ? "DEBUG: ON" : "DEBUG: OFF");
        debugButton.OnClick += DebugButton_OnClick;
        scrollBox.AddItems(debugButton);

        OpLabel regionHeading = new OpLabel(new Vector2(290f, scrollHeight - 20f), new Vector2(), "REGION SETTINGS", FLabelAlignment.Center, true);
        OpLabel regionDesc = new OpLabel(new Vector2(290f, scrollHeight - 73f), new Vector2(), "Disable weather for certain regions using the checkboxes.\n\nIf a region has it's own custom weather settings, you can use\nthe buttons on the right to override them and use your Global settings instead.", FLabelAlignment.Center, false); ;
        scrollBox.AddItems(regionHeading, regionDesc);

        for (int i = 0; i < array.Length; i++)
        {
            OpRect rect = new OpRect(new Vector2(0f, itemHeight - 50f - (70f * i)), new Vector2(580f, 60f));
            regionRects.Add(rect);
            OpLabel regionName = new OpLabel(20f, itemHeight - 34f - (70f * i), array[i] + " - " + Region.GetRegionFullName(array[i], SlugcatStats.Name.White), true);
            regionLabels.Add(regionName);
            OpSimpleButton weatherSwitch = new OpSimpleButton(new Vector2(470f, itemHeight - 40f - (70f * i)), new Vector2(100f, 40f), "GLOBAL");
            weatherSwitch.OnClick += WeatherSwitch_OnClick;
            weatherSwitch.description = $"{i}.{array[i]}) Change this region's weather settings";
            regionButtons.Add(weatherSwitch);
            scrollBox.AddItems(rect, regionName, weatherSwitch);
            if (customRegionSettings.ContainsKey(array[i]))
            {
                regionName.SetPos(regionName.GetPos() + new Vector2(0f, 11f));
            }
            OpLabel customLabel = new OpLabel(20f, itemHeight - 42f - (70f * i), customRegionSettings.ContainsKey(array[i]) ? "This region has custom settings" : "", false);
            customLabels.Add(customLabel);
            scrollBox.AddItems(customLabel);

            weatherSwitch.text = RegionModText(regionSettings[array[i]]);
            weatherSwitch.colorEdge = RegionButtonColor(regionSettings[array[i]]);
            rect.colorEdge = weatherSwitch.colorEdge;
            rect.colorFill = weatherSwitch.colorEdge;
            customLabel.color = weatherSwitch.colorEdge;
            regionName.color = weatherSwitch.colorEdge;
        }
        #endregion

        ForecastLog.Log($"Support Mode: {(supportMode.Value ? "ON" : "OFF")}");
        OnConfigReset += ForecastConfig_OnConfigReset;
    }

    private void DebugButton_OnClick(UIfocusable trigger)
    {
        if (debugMode.Value)
        {
            debugMode.Value = false;
        }
        else
        {
            debugMode.Value = true;
        }
        config.Save();
    }

    private void WaterCollisionToggle_OnClick(UIfocusable trigger)
    {
        if (waterCollision.Value)
        {
            waterCollision.Value = false;
        }
        else
        {
            waterCollision.Value = true;
        }
        config.Save();
    }

    private void BackgroundCollisionToggle_OnClick(UIfocusable trigger)
    {
        if (backgroundCollision.Value)
        {
            backgroundCollision.Value = false;
        }
        else
        {
            backgroundCollision.Value = true;
        }
        config.Save();
    }

    private void StrikeTypeToggle_OnClick(UIfocusable trigger)
    {
        if (strikeDamageType.Value == 2)
        {
            strikeDamageType.Value = 0;
        }
        else
        {
            strikeDamageType.Value++;
        }
        config.Save();
    }

    private void StrikeToggle_OnClick(UIfocusable trigger)
    {
        if (lightningStrikes.Value)
        {
            lightningStrikes.Value = false;
        }
        else
        {
            lightningStrikes.Value = true;
        }
        config.Save();
    }

    private void ForecastConfig_OnConfigReset()
    {
        weatherIntensity.Value = 0;
        weatherChance.Value = 100;
        windDirection.Value = 0;

        particleLimit.Value = 100;

        backgroundCollision.Value = true;
        waterCollision.Value = true;
        //dynamicClouds.Value = true;

        backgroundLightning.Value = true;
        lightningStrikes.Value = true;
        lightningInterval.Value = 10;
        lightningChance.Value = 15;
        strikeDamageType.Value = 1;

        config.Save();
    }

    private void BgToggle_OnClick(UIfocusable trigger)
    {
        if (backgroundLightning.Value)
        {
            backgroundLightning.Value = false;
        }
        else
        {
            backgroundLightning.Value = true;
        }
        config.Save();
    }

    private void WindToggle_OnClick(UIfocusable trigger)
    {
        if (windDirection.Value == 3)
        {
            windDirection.Value = 0;
        }
        else
        {
            windDirection.Value++;
        }
        config.Save();
    }

    private void IntensityToggle_OnClick(UIfocusable trigger)
    {
        if (weatherIntensity.Value == 3)
        {
            weatherIntensity.Value = 0;
        }
        else
        {
            weatherIntensity.Value++;
        }
        config.Save();
    }

    private void SupportModeButton_OnClick(UIfocusable trigger)
    {
        if (supportMode.Value)
        {
            supportMode.Value = false;
        }
        else
        {
            supportMode.Value = true;
        }
        config.Save();
    }

    private void WeatherSwitch_OnClick(UIfocusable trigger)
    {
        int index = int.Parse(trigger.description.Split('.')[0]);
        string region = trigger.description.Split('.')[1].Split(')')[0];
        if (customRegionSettings.ContainsKey(region))
        {
            regionSettings[region]++;
            if (regionSettings[region] == 3) { regionSettings[region] = 0; }
        }
        else
        {
            regionSettings[region]++;
            if (regionSettings[region] == 2) { regionSettings[region] = 0; }
        }

        regionButtons[index].text = RegionModText(regionSettings[region]);
        regionButtons[index].colorEdge = RegionButtonColor(regionSettings[region]);
        regionRects[index].colorEdge = regionButtons[index].colorEdge;
        regionRects[index].colorFill = regionButtons[index].colorEdge;
        customLabels[index].color = regionButtons[index].colorEdge;
        regionLabels[index].color = regionButtons[index].colorEdge;

        SaveRegionWeather();
    }

    private Color RegionButtonColor(int val)
    {
        switch (val)
        {
            case 0:
                return new Color(0.65f, 0.25f, 0.25f);
            case 1:
                return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            case 2:
                return new Color(0.1f, 0.8f, 0.1f);
        }
        return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
    }

    private string RegionModText(int val)
    {
        switch (val)
        {
            case 0:
                return "NONE";
            case 1:
                return "GLOBAL";
            case 2:
                return "CUSTOM";
        }
        return "ERROR";
    }

    private string StrikeDamageValue()
    {
        switch (strikeDamageType.Value)
        {
            case 0:
                return "NONE";
            case 1:
                return "STUN";
            case 2:
                return "LETHAL";
        }
        return "";
    }

    private string IntensityValue()
    {
        switch (weatherIntensity.Value)
        {
            case 0:
                return "DYNAMIC";
            case 1:
                return "LOW";
            case 2:
                return "MEDIUM";
            case 3:
                return "HIGH";
        }
        return "";
    }

    private string WindDirectionValue()
    {
        switch (windDirection.Value)
        {
            case 0:
                return "RANDOM";
            case 1:
                return "LEFT";
            case 2:
                return "CENTER";
            case 3:
                return "RIGHT";
        }
        return "";
    }

    public override void Update()
    {
        base.Update();

        if (!supportMode.Value)
        {
            settingsBox.greyedOut = false;
            foreach (UIelement ui in settingsBox.items)
            {
                ui.Reactivate();
            }
            supportWarning.Deactivate();
            supportWarningDesc.Deactivate();
        }
        else
        {
            settingsBox.greyedOut = true;
            foreach (UIelement ui in settingsBox.items)
            {
                ui.Deactivate();
            }
            supportWarning.Reactivate();
            supportWarningDesc.Reactivate();
        }
        supportRect.colorEdge = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
        supportRect.colorFill = supportMode.Value ? new Color(0f, 0.5f, 0f) : new Color(0f, 0f, 0f);
        supportModeButton.text = supportMode.Value ? "ENABLED" : "DISABLED";
        bgToggle.text = backgroundLightning.Value ? "ENABLED" : "DISABLED";
        strikeToggle.text = lightningStrikes.Value ? "ENABLED" : "DISABLED";
        waterCollisionToggle.text = waterCollision.Value ? "ENABLED" : "DISABLED";
        backgroundCollisionToggle.text = backgroundCollision.Value ? "ENABLED" : "DISABLED";
        debugButton.text = debugMode.Value ? "DEBUG: ON" : "DEBUG: OFF";
        windToggle.text = WindDirectionValue();
        intensityToggle.text = IntensityValue();
        strikeTypeToggle.text = StrikeDamageValue();

        if (!lightningStrikes.Value)
        {
            strikeTypeToggle.greyedOut = true;
            intervalSlider.greyedOut = true;
            strikeChanceSlider.greyedOut = true;
        }
        else
        {
            strikeTypeToggle.greyedOut = false;
            intervalSlider.greyedOut = false;
            strikeChanceSlider.greyedOut = false;
        }
    }
}