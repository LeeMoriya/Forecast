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

    public static Configurable<bool> weatherPreference;
    public static Configurable<int> weatherRandomness;

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
    public static Configurable<int> strikeWeathers;
    public static Configurable<Color> strikeColor;
    public static Configurable<bool> greenLightning;

    public static Configurable<bool> endBlizzard;
    public static Configurable<bool> effectColors;
    public static Configurable<bool> snowPuffs;
    public static Configurable<bool> snowSources;
    public static Configurable<bool> classicSnow;
    public static Configurable<int> coldFactor;
    public static Configurable<int> windSpeed;
    public static Configurable<bool> vignette;

    public static Configurable<bool> debugMode;

    //Manual Configurables
    public static Dictionary<string, int> regionSettings = new Dictionary<string, int>();
    public static Dictionary<string, Dictionary<string, List<string>>> customRegionSettings;
    public static Dictionary<WeatherForecast.Weather.WeatherType, float> copiedWeather;

    //Menu
    public OpImage rainBanner;
    public OpRect supportRect;
    public OpSimpleButton supportModeButton;
    public bool init = false;

    public UIelement[] settings;
    public OpSimpleButton forecastEdit;
    public OpSimpleButton preferenceToggle;
    public OpSlider randomnessSlider;
    public OpSimpleButton intensityToggle;
    public OpSimpleButton windToggle;
    public OpScrollBox settingsBox;
    public OpSimpleButton bgToggle;
    public OpSimpleButton strikeToggle;
    public OpSimpleButton greenToggle;
    public OpLabel supportWarning;
    public OpLabel supportWarningDesc;
    public OpSimpleButton strikeTypeToggle;
    public OpSimpleButton strikeWeatherToggle;
    public OpSlider intervalSlider;
    public OpSlider strikeChanceSlider;
    public OpSimpleButton backgroundCollisionToggle;
    public OpSimpleButton waterCollisionToggle;
    public List<OpLabel> weatherChances;
    public List<OpImage> weatherIcons;
    public List<OpSimpleImageButton> forecastButtons;
    public OpSimpleButton snowSourceToggle;
    public OpSimpleButton vignetteToggle;

    public OpSimpleButton snowSlider;
    public OpRect selector;
    public OpLabel classicLabel, downpourLabel;
    public OpSlider coldSlider;
    public OpSlider windSlider;

    public static bool preferenceUpdate = false;
    public static bool updateRegionSettingsButtons = false;

    //Debug
    public OpSimpleButton debugButton;

    //Region
    List<OpRect> regionRects;
    List<OpLabel> regionLabels;
    List<OpSimpleButton> regionButtons;
    List<OpLabel> customLabels;
    public OpSimpleButton forecastDialogButton;

    public ForecastConfig(ForecastMod mod)
    {
        WeatherData.Load();

        //regionSettings = new Dictionary<string, int>();
        customRegionSettings = new Dictionary<string, Dictionary<string, List<string>>>();

        weatherType = config.Bind<int>("weatherType", 0);
        supportMode = config.Bind<bool>("supportMode", false);

        weatherIntensity = config.Bind<int>("weatherIntensity", 0, new ConfigAcceptableRange<int>(0, 3));
        weatherChance = config.Bind<int>("weatherChance", 100, new ConfigAcceptableRange<int>(0, 100));
        windDirection = config.Bind<int>("windDirection", 0, new ConfigAcceptableRange<int>(0, 3));

        weatherPreference = config.Bind<bool>("weatherPreference", true);
        weatherRandomness = config.Bind<int>("weatherRandomness", 10, new ConfigAcceptableRange<int>(0, 100));

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
        strikeWeathers = config.Bind<int>("strikeWeathers", 0);
        strikeColor = config.Bind<Color>("strikeColor", new Color(1f, 1f, 0.95f, 1f));
        greenLightning = config.Bind<bool>("greenLightning", true);

        endBlizzard = config.Bind<bool>("endBlizzard", true);
        effectColors = config.Bind<bool>("effectColors", true);
        snowPuffs = config.Bind<bool>("snowPuffs", true);
        snowSources = config.Bind<bool>("snowSources", true);
        classicSnow = config.Bind<bool>("classicSnow", true);
        coldFactor = config.Bind<int>("coldFactor", 5, new ConfigAcceptableRange<int>(0, 10));
        windSpeed = config.Bind<int>("windSpeed", 10, new ConfigAcceptableRange<int>(0, 10));
        vignette = config.Bind<bool>("vignette", true);

        debugMode = config.Bind<bool>("debugMode", false);
        LoadCustomRegionSettings();
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
                        regionSettings[array[i]] = 3;
                        ForecastLog.Log("Switch to 3");
                    }
                    else if (supportMode.Value)
                    {
                        regionSettings[array[i]] = 3;
                        ForecastLog.Log("Switch to 3 - support mode");
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
        updateRegionSettingsButtons = true;

        //Debug - print loaded tags
        //foreach (string reg in customRegionSettings.Keys)
        //{
        //    foreach (string key in customRegionSettings[reg].Keys)
        //    {
        //        string tags = "";
        //        foreach (string tag in customRegionSettings[reg][key])
        //        {
        //            tags += tag;
        //            tags += " ";
        //        }
        //        ForecastLog.Log("FORECAST: " + key + ": " + tags);
        //    }
        //}
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
        OpLabel version = new OpLabel(300f, 525f, $"Version: {ForecastMod.versionNum}     -     By LeeMoriya", false);
        version.color = new Color(0.4f, 0.4f, 0.4f);
        version.label.alignment = FLabelAlignment.Center;
        options.AddItems(version, rainBanner);

        //Support Mode
        supportRect = new OpRect(new Vector2(10f, 420f), new Vector2(580f, 80f));
        supportRect.colorEdge = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
        supportRect.colorFill = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0f, 0f, 0f);
        OpLabel supportTitle = new OpLabel(165f, 470f, "SUPPORT MODE");
        OpLabel supportDesc = new OpLabel(165f, 440f, "When support mode is active, Forecast will not generate any weather\nunless a region has custom settings defined by a mod.");

        supportModeButton = new OpSimpleButton(new Vector2(33f, 437f), new Vector2(110f, 45f), supportMode.Value ? "ENABLED" : "DISABLED");
        supportModeButton.OnClick += SupportModeButton_OnClick;
        options.AddItems(supportRect, supportTitle, supportDesc, supportModeButton);

        float settingsHeight = 2300f;
        settingsBox = new OpScrollBox(new Vector2(0f, 0f), new Vector2(600f, 400f), settingsHeight, false, true, true);
        options.AddItems(settingsBox);

        OpLabel globalSettings = new OpLabel(new Vector2(290f, settingsHeight - 35f), new Vector2(), "GLOBAL SETTINGS", FLabelAlignment.Center, true);
        OpLabel globalDesc = new OpLabel(new Vector2(290f, settingsHeight - 70f), new Vector2(), "Define settings that apply to all areas where weather is enabled, including global forecasts.\nYou can configure different forecasts per region in the 'Regions' tab.", FLabelAlignment.Center);

        //GLOBAL FORECAST
        float globalForecastAnchor = settingsHeight - 130f;

        OpLabel forecastLabel = new OpLabel(new Vector2(290f, globalForecastAnchor + 15f), new Vector2(), "- FORECAST SETTINGS -", FLabelAlignment.Center);
        OpRect forecastRect = new OpRect(new Vector2(15f, globalForecastAnchor - 240f), new Vector2(555f, 250f));
        forecastRect.colorFill = new Color(0.8f, 0.8f, 0.8f);
        settingsBox.AddItems(forecastLabel, forecastRect);

        weatherIcons = new List<OpImage>();
        weatherChances = new List<OpLabel>();
        int numOfWeathers = 7; //We do a little hardcoding
        float startPos = 320f - 50f * (numOfWeathers / 2);

        forecastEdit = new OpSimpleButton(new Vector2(30f, globalForecastAnchor - 58f), new Vector2(110f, 45f), "EDIT");
        forecastEdit.OnClick += ForecastEdit_OnClick;
        settingsBox.AddItems(forecastEdit);

        for (int i = 0; i < 7; i++)
        {
            OpImage icon = new OpImage(new Vector2(startPos + (55f * i), globalForecastAnchor -40f), WeatherForecast.regionWeatherProbability["GLOBAL"].ElementAt(i).Key.ToString());
            OpLabel label = new OpLabel(startPos + 7f + (55f * i), globalForecastAnchor - 65f, weatherPreference.Value ? "-" : $"{Mathf.Round(WeatherForecast.regionWeatherProbability["GLOBAL"].ElementAt(i).Value * 100f).ToString()}%", false);
            label.alignment = FLabelAlignment.Center;
            if(WeatherForecast.regionWeatherProbability["GLOBAL"].ElementAt(i).Value == 0f)
            {
                icon.color = new Color(0.3f, 0.3f, 0.3f);
            }
            weatherIcons.Add(icon);
            weatherChances.Add(label);
            settingsBox.AddItems(icon, label);
        }

        preferenceToggle = new OpSimpleButton(new Vector2(30f, globalForecastAnchor - 140f), new Vector2(110f, 45f), weatherPreference.Value ? "ENABLED" : "DISABLED");
        preferenceToggle.OnClick += PreferenceToggle_OnClick;
        OpLabel preferenceLabel = new OpLabel(160f, globalForecastAnchor - 118f, "WEATHER PREFERENCE");
        OpLabel preferenceDesc = new OpLabel(160f, globalForecastAnchor - 138f, "Weathers transition more realistically - overwrites above chances");

        randomnessSlider = new OpSlider(weatherRandomness, new Vector2(30f, globalForecastAnchor - 210f), 110, false);
        OpLabel randomnessLabel = new OpLabel(160f, globalForecastAnchor - 190f, "WEATHER RANDOMNESS");
        OpLabel randomnessDesc = new OpLabel(160f, globalForecastAnchor - 210f, "Chance that preference will be ignored and a random weather is chosen");
        settingsBox.AddItems(preferenceToggle, randomnessSlider, preferenceLabel, preferenceDesc, randomnessLabel, randomnessDesc);

        //BASIC SETTINGS
        float basicAnchor = globalForecastAnchor - 300f;

        OpLabel basicLabel = new OpLabel(new Vector2(280f, basicAnchor + 15f), new Vector2(), "- BASIC SETTINGS -", FLabelAlignment.Center);

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
        float visualAnchor = basicAnchor - 370f;
        OpLabel visualLabel = new OpLabel(new Vector2(280f, visualAnchor + 15f), new Vector2(), "- VISUAL SETTINGS -", FLabelAlignment.Center);

        OpRect visualRect = new OpRect(new Vector2(15f, visualAnchor - 233.5f), new Vector2(555, 240f));
        visualRect.colorFill = new Color(1f, 0f, 1f);
        settingsBox.AddItems(visualRect, visualLabel);

        //Background Lightning
        OpLabel bgLabel = new OpLabel(160f, visualAnchor - 30f, "BACKGROUND LIGHTNING");
        OpLabel bgDesc = new OpLabel(160f, visualAnchor - 50f, "During thunderstorms, lightning flashes can occur");
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
        float lightningAnchor = visualAnchor - 290f;
        OpLabel lightningLabel = new OpLabel(new Vector2(280f, lightningAnchor + 15f), new Vector2(), "- LIGHTNING SETTINGS -", FLabelAlignment.Center);

        OpRect lightningSettingsRect = new OpRect(new Vector2(15f, lightningAnchor - 483.5f), new Vector2(555f, 490f));
        lightningSettingsRect.colorFill = new Color(1f, 1f, 0f);

        settingsBox.AddItems(lightningLabel, lightningSettingsRect);

        //Lightning Strikes
        OpLabel strikeLabel = new OpLabel(160f, lightningAnchor - 30f, "LIGHTNING STRIKES");
        OpLabel strikeDesc = new OpLabel(160f, lightningAnchor - 50f, "Lightning strikes can occur during thunderstorms");
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

        //Strike Damage
        OpLabel strikeTypeLabel = new OpLabel(160f, lightningAnchor - 270f, "DAMAGE TYPE");
        OpLabel strikeTypeDesc = new OpLabel(160f, lightningAnchor - 290f, "What type of damage a lightning strike will inflict upon hit");
        strikeTypeToggle = new OpSimpleButton(new Vector2(30f, lightningAnchor - 295f), new Vector2(110f, 45f), StrikeDamageValue());
        strikeTypeToggle.OnClick += StrikeTypeToggle_OnClick;
        settingsBox.AddItems(strikeTypeLabel, strikeTypeDesc, strikeTypeToggle);

        //Strike Weathers
        OpLabel strikeWeatherLabel = new OpLabel(160f, lightningAnchor - 350f, "WEATHER TYPES");
        OpLabel strikeWeatherDesc = new OpLabel(160f, lightningAnchor - 370f, "The weather types lightning strikes can occur in");
        strikeWeatherToggle = new OpSimpleButton(new Vector2(30f, lightningAnchor - 375f), new Vector2(110f, 45f), StrikeWeathers());
        strikeWeatherToggle.OnClick += StrikeWeatherToggle_OnClick;
        settingsBox.AddItems(strikeWeatherLabel, strikeWeatherDesc, strikeWeatherToggle);

        //Green Strikes
        OpLabel greenStrikeLabel = new OpLabel(160f, lightningAnchor - 430f, "GREEN LIGHTNING");
        OpLabel greenStrikeDesc = new OpLabel(160f, lightningAnchor - 450f, "Makes lighting strikes green in Shaded Citadel and The Exterior");
        greenToggle = new OpSimpleButton(new Vector2(30f, lightningAnchor - 455), new Vector2(110f, 45f), greenLightning.Value ? "ENABLED" : "DISABLED");
        greenToggle.OnClick += GreenToggle_OnClick;
        settingsBox.AddItems(greenStrikeLabel, greenStrikeDesc, greenToggle);

        //SNOW SETTINGS
        float snowAnchor = lightningAnchor - 540f;
        OpLabel snowLabel = new OpLabel(new Vector2(280f, snowAnchor + 15f), new Vector2(), "- SNOW SETTINGS -", FLabelAlignment.Center);

        OpRect snowSettingsRect = new OpRect(new Vector2(15f, snowAnchor - 383.5f), new Vector2(555f, 390f));
        snowSettingsRect.colorFill = new Color(0.5f, 1f, 1f);

        settingsBox.AddItems(snowLabel, snowSettingsRect);

        //Classic Slider
        snowSlider = new OpSimpleButton(new Vector2(30f, snowAnchor - 50f), new Vector2(520f, 35f));
        snowSlider.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        snowSlider.OnGrafUpdate += SnowSlider_OnGrafUpdate;
        snowSlider.OnClick += SnowSliderToggle_OnClick;
        selector = new OpRect(new Vector2(35f, snowAnchor - 45f), new Vector2(230f, 25f));
        selector.fillAlpha = 1f;
        selector.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        classicLabel = new OpLabel(130f, snowAnchor - 42f, "FORECAST", false);
        classicLabel.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
        downpourLabel = new OpLabel(390f, snowAnchor - 42f, "DOWNPOUR", false);
        downpourLabel.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        settingsBox.AddItems(snowSlider, selector, classicLabel, downpourLabel);

        //Snow Sources
        OpLabel snowSourceLabel = new OpLabel(160f, snowAnchor - 100f, "SNOW SOURCES");
        OpLabel snowSourceDesc = new OpLabel(160f, snowAnchor - 120f, "Dynamically places snow sources during snowy weather");
        snowSourceToggle = new OpSimpleButton(new Vector2(30f, snowAnchor - 125f), new Vector2(110f, 45f), snowSources.Value ? "ENABLED" : "DISABLED");
        snowSourceToggle.OnClick += SnowSourceToggle_OnClick;
        settingsBox.AddItems(snowSourceLabel, snowSourceDesc, snowSourceToggle);

        //Cold Factor
        coldSlider = new OpSlider(coldFactor, new Vector2(30f, snowAnchor - 190f), 110, false);
        OpLabel coldFactorLabel = new OpLabel(160f, snowAnchor - 180f, "COLD FACTOR");
        OpLabel coldFactorDesc = new OpLabel(160f, snowAnchor - 200f, "Configure the rate at which you grown cold in Forecast style blizzards");
        settingsBox.AddItems(coldSlider, coldFactorLabel, coldFactorDesc);

        //Wind Speed
        windSlider = new OpSlider(windSpeed, new Vector2(30f, snowAnchor - 260f), 110, false);
        OpLabel windSpeedLabel = new OpLabel(160f, snowAnchor - 250f, "WIND SPEED");
        OpLabel windSpeedDesc = new OpLabel(160f, snowAnchor - 270f, "How easily the player is pushed by wind during Forecast style blizzards");
        settingsBox.AddItems(windSlider, windSpeedLabel, windSpeedDesc);

        //Snow Sources
        OpLabel vignetteLabel = new OpLabel(160f, snowAnchor - 320f, "ICY BORDER");
        OpLabel vignetteDesc = new OpLabel(160f, snowAnchor - 340f, "Toggle an icy border that indicates coldness");
        vignetteToggle = new OpSimpleButton(new Vector2(30f, snowAnchor - 345f), new Vector2(110f, 45f), vignette.Value ? "ENABLED" : "DISABLED");
        vignetteToggle.OnClick += VignetteToggle_OnClick;
        settingsBox.AddItems(vignetteLabel, vignetteDesc, vignetteToggle);

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
        forecastButtons = new List<OpSimpleImageButton>();

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
        OpLabel regionDesc = new OpLabel(new Vector2(290f, scrollHeight - 73f), new Vector2(), "Adjust whether a region will use the GLOBAL forecast you have set or define a CUSTOM one.\nTo fully disable weather, change it's setting to NONE.\n\nIf a region has specific settings configured by a mod-maker, it can be overridden here.", FLabelAlignment.Center, false); ;
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

            OpSimpleImageButton forecastButton = new OpSimpleImageButton(weatherSwitch.pos + new Vector2(-45f, 5f), new Vector2(30f, 30f), "forecastcog");
            forecastButton.OnClick += ForecastButton_OnClick;
            forecastButton.sprite.x -= 1f;
            forecastButton.sprite.y -= 1f;
            forecastButton.greyedOut = regionSettings[array[i]] != 2;
            forecastButton.description = array[i];
            forecastButtons.Add(forecastButton);

            scrollBox.AddItems(rect, regionName, weatherSwitch, forecastButton);
            if (customRegionSettings.ContainsKey(array[i]))
            {
                regionName.SetPos(regionName.GetPos() + new Vector2(0f, 11f));
            }
            OpLabel customLabel = new OpLabel(20f, itemHeight - 42f - (70f * i), customRegionSettings.ContainsKey(array[i]) ? "This region has it's own modded settings" : "", false);
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

        //UpdateRegionSettingsButtons();

        ForecastLog.Log($"Support Mode: {(supportMode.Value ? "ON" : "OFF")}");
        OnConfigReset += ForecastConfig_OnConfigReset;
    }

    private void SnowSlider_OnGrafUpdate(float timeStacker)
    {
        selector.fillAlpha = Mathf.Lerp(1f,0.7f,snowSlider.bumpBehav.flash);
    }

    private void ForecastButton_OnClick(UIfocusable trigger)
    {
        var rw = GameObject.FindObjectOfType<RainWorld>();
        Dialog dialog = new ForecastDialog2(rw.processManager, trigger.description);
        rw.processManager.ShowDialog(dialog);
    }

    private void ForecastEdit_OnClick(UIfocusable trigger)
    {
        var rw = GameObject.FindObjectOfType<RainWorld>();
        Dialog dialog = new ForecastDialog2(rw.processManager, "GLOBAL");
        rw.processManager.ShowDialog(dialog);
    }

    private void ToggleSetting(Func<bool> getValue, Action<bool> setValue, bool updatePreference = false)
    {
        setValue(!getValue());
        config.Save();
        if (updatePreference) preferenceUpdate = true;
    }

    private void PreferenceToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => weatherPreference.Value, v => weatherPreference.Value = v, true);
    private void DebugButton_OnClick(UIfocusable trigger) => ToggleSetting(() => debugMode.Value, v => debugMode.Value = v);
    private void WaterCollisionToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => waterCollision.Value, v => waterCollision.Value = v);
    private void BackgroundCollisionToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => backgroundCollision.Value, v => backgroundCollision.Value = v);
    private void StrikeToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => lightningStrikes.Value, v => lightningStrikes.Value = v);
    private void BgToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => backgroundLightning.Value, v => backgroundLightning.Value = v);
    private void SupportModeButton_OnClick(UIfocusable trigger) => ToggleSetting(() => supportMode.Value, v => supportMode.Value = v);
    private void GreenToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => greenLightning.Value, v => greenLightning.Value = v);
    private void SnowSourceToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => snowSources.Value, v => snowSources.Value = v);
    private void SnowSliderToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => classicSnow.Value, v => classicSnow.Value = v);
    private void VignetteToggle_OnClick(UIfocusable trigger) => ToggleSetting(() => vignette.Value, v => vignette.Value = v);




    private void StrikeWeatherToggle_OnClick(UIfocusable trigger)
    {
        if (strikeWeathers.Value == 2)
        {
            strikeWeathers.Value = 0;
        }
        else
        {
            strikeWeathers.Value++;
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

    private void WeatherSwitch_OnClick(UIfocusable trigger)
    {
        int index = int.Parse(trigger.description.Split('.')[0]);
        string region = trigger.description.Split('.')[1].Split(')')[0];
        if (customRegionSettings.ContainsKey(region))
        {
            regionSettings[region]++;
            if (regionSettings[region] == 4) { regionSettings[region] = 0; }
        }
        else
        {
            regionSettings[region]++;
            if (regionSettings[region] == 3) { regionSettings[region] = 0; }
        }

        regionButtons[index].text = RegionModText(regionSettings[region]);
        regionButtons[index].colorEdge = RegionButtonColor(regionSettings[region]);
        regionRects[index].colorEdge = regionButtons[index].colorEdge;
        regionRects[index].colorFill = regionButtons[index].colorEdge;
        customLabels[index].color = regionButtons[index].colorEdge;
        regionLabels[index].color = regionButtons[index].colorEdge;

        forecastButtons[index].greyedOut = regionSettings[region] != 2;
        //updateRegionSettingsButtons = true;
        SaveRegionWeather();
    }

    private void UpdateRegionSettingsButtons()
    {
        for (int i = 0; i < regionSettings.Count; i++)
        {
            int val = regionSettings.ElementAt(i).Value;
            regionButtons[i].text = RegionModText(val);
            regionButtons[i].colorEdge = RegionButtonColor(val);
            regionRects[i].colorEdge = regionButtons[i].colorEdge;
            regionRects[i].colorFill = regionButtons[i].colorEdge;
            customLabels[i].color = regionButtons[i].colorEdge;
            regionLabels[i].color = regionButtons[i].colorEdge;
            forecastButtons[i].greyedOut = val != 2;
            ForecastLog.Log($"Updating region button for: {regionSettings.ElementAt(i).Key} = {val}");
        }
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
            case 3:
                return new Color(0.1f, 0.8f, 0.8f);
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
            case 3:
                return "MOD";
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

    private string StrikeWeathers()
    {
        switch (strikeWeathers.Value)
        {
            case 0:
                return "THUNDERSTORM";
            case 1:
                return "THUNDERSTORM\n& BLIZZARD";
            case 2:
                return "ALL";
        }
        return "";
    }

    private string IntensityValue()
    {
        switch (weatherIntensity.Value)
        {
            case 0:
                return "DYNAMIC\n(Recommended)";
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
        preferenceToggle.text = weatherPreference.Value ? "ENABLED" : "DISABLED";
        greenToggle.text = greenLightning.Value ? "ENABLED" : "DISABLED";
        snowSourceToggle.text = snowSources.Value ? "ENABLED" : "DISABLED";
        windToggle.text = WindDirectionValue();
        intensityToggle.text = IntensityValue();
        strikeTypeToggle.text = StrikeDamageValue();
        strikeWeatherToggle.text = StrikeWeathers();
        classicLabel.color = classicSnow.Value ? Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey) : Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        downpourLabel.color = classicSnow.Value ? Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey) : Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey);
        selector.PosX = classicSnow.Value ? 35f : 315f;

        randomnessSlider.greyedOut = !weatherPreference.Value;

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
            vignetteToggle.greyedOut = !classicSnow.Value;
            windSlider.greyedOut = !classicSnow.Value;
            coldSlider.greyedOut = !classicSnow.Value;

        if (preferenceUpdate)
        {
            for (int i = 0; i < weatherChances.Count; i++)
            {
                float val = WeatherForecast.regionWeatherProbability["GLOBAL"].ElementAt(i).Value;
                weatherIcons[i].color = val != 0f ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.3f, 0.3f, 0.3f);
                weatherChances[i].text = weatherPreference.Value ? "-" : $"{Mathf.Round(val * 100f).ToString()}%";
            }
            preferenceUpdate = false;
        }
        if (updateRegionSettingsButtons)
        {
            //UpdateRegionSettingsButtons();
            updateRegionSettingsButtons = false;
        }
    }
}