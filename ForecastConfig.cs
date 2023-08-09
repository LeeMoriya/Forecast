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
    public static Configurable<float> weatherChance;
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
    public static Configurable<float> lightningChance;
    public static Configurable<int> strikeDamageType;
    public static Configurable<Color> strikeColor;

    public static Configurable<bool> endBlizzard;
    public static Configurable<bool> effectColors;
    public static Configurable<bool> snowPuffs;

    //Manual Configurables
    public static Dictionary<string, int> regionSettings;
    public static Dictionary<string, Dictionary<string, List<string>>> customRegionSettings;

    //Menu
    public OpImage rainBanner;
    public OpRect supportRect;
    public OpSimpleButton supportModeButton;

    //Region
    List<OpRect> regionRects;
    List<OpLabel> regionLabels;
    List<OpCheckBox> regionCheckBoxes;
    List<OpSimpleButton> regionButtons;

    public ForecastConfig(Forecast mod)
    {
        regionSettings = new Dictionary<string, int>();
        customRegionSettings = new Dictionary<string, Dictionary<string, List<string>>>();
        LoadCustomRegionSettings();

        weatherType = config.Bind<int>("weatherType", 0);
        supportMode = config.Bind<bool>("supportMode", false);

        weatherIntensity = config.Bind<int>("weatherIntensity", 0);
        weatherChance = config.Bind<float>("weatherChance", 100f);
        windDirection = config.Bind<int>("windDirection", 0);

        particleLimit = config.Bind<int>("particleLimit", 100);

        backgroundCollision = config.Bind<bool>("backgroundCollision", true);
        waterCollision = config.Bind<bool>("waterCollision", true);
        dynamicClouds = config.Bind<bool>("dynamicClouds", true);
        cloudCover = config.Bind<float>("cloudCover", 0.5f);

        rainVolume = config.Bind<bool>("rainVolume", true);

        backgroundLightning = config.Bind<bool>("backgroundLightning", true);
        lightningStrikes = config.Bind<bool>("lightningStrikes", true);
        lightningInterval = config.Bind<int>("lightningInterval", 10);
        lightningChance = config.Bind<float>("lightningChance", 0.15f);
        strikeDamageType = config.Bind<int>("strikeDamageType", 0);
        strikeColor = config.Bind<Color>("strikeColor", new Color(1f, 1f, 0.95f, 1f));

        endBlizzard = config.Bind<bool>("endBlizzard", true);
        effectColors = config.Bind<bool>("effectColors", true);
        snowPuffs = config.Bind<bool>("snowPuffs", true);
    }

    public static void LoadCustomRegionSettings()
    {
        string[] array = new string[]
        {
            ""
        };
        string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
        if (File.Exists(path))
        {
            array = File.ReadAllLines(path);
        }

        for (int i = 0; i < array.Length; i++)
        {
            if(File.Exists(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + array[i] + Path.DirectorySeparatorChar + array[i] + "_forecast.txt")))
            {
                ForecastLog.Log($"FORECAST: Custom settings found for {array[i]}");

                string[] data = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + array[i] + Path.DirectorySeparatorChar + array[i] + "_forecast.txt"));
                if (!customRegionSettings.ContainsKey(array[i]))
                {
                    customRegionSettings.Add(array[i], new Dictionary<string, List<string>>());
                }
                bool globalSettings = false;
                bool roomSection = false;
                for (int s = 0; s < data.Length; s++)
                {
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

        foreach(string reg in customRegionSettings.Keys)
        {
            foreach(string key in customRegionSettings[reg].Keys)
            {
                string tags = "";
                foreach(string tag in customRegionSettings[reg][key])
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
        OpLabel version = new OpLabel(300f, 525f, $"Version: {Forecast.version}     -     By LeeMoriya", false);
        version.color = new Color(0.4f, 0.4f, 0.4f);
        version.label.alignment = FLabelAlignment.Center;
        options.AddItems(version, rainBanner);

        //Support Mode
        supportRect = new OpRect(new Vector2(10f, 410f), new Vector2(580f, 80f));
        supportRect.colorEdge = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
        supportRect.colorFill = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0f, 0f, 0f);
        OpLabel supportTitle = new OpLabel(41f, 495f, "SUPPORT MODE");
        OpLabel supportDesc = new OpLabel(165f, 440f, "When support mode is active, Forecast will not generate any weather\nunless a region has custom settings defined.");

        supportModeButton = new OpSimpleButton(new Vector2(33f, 427f), new Vector2(110f, 45f), supportMode.Value ? "DISABLE" : "ENABLE");
        supportModeButton.OnClick += SupportModeButton_OnClick;
        options.AddItems(supportRect, supportTitle ,supportDesc, supportModeButton);

        OpRect settingsRect = new OpRect(new Vector2(0f, 0f), new Vector2(600f, 400f));
        options.AddItems(settingsRect);

        #endregion

        #region Regions Tab

        regionRects = new List<OpRect>();
        regionLabels = new List<OpLabel>();
        regionCheckBoxes = new List<OpCheckBox>();
        regionButtons = new List<OpSimpleButton>();

        string[] array = new string[]
        {
            ""
        };
        string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
        if (File.Exists(path))
        {
            array = File.ReadAllLines(path);
        }

        float scrollHeight = 70f * array.Length;
        float itemHeight = scrollHeight - 15f;
        OpScrollBox scrollBox = new OpScrollBox(regions, scrollHeight, false, true);
        regions.AddItems(scrollBox);

        for (int i = 0; i < array.Length; i++)
        {
            if (!regionSettings.ContainsKey(array[i]))
            {
                regionSettings.Add(array[i], 0);
            }

            OpRect rect = new OpRect(new Vector2(0f, itemHeight - 50f - (70f * i)), new Vector2(580f, 60f));
            regionRects.Add(rect);
            OpLabel regionName = new OpLabel(50f, itemHeight - 32f - (70f * i), array[i] + " - " + Region.GetRegionFullName(array[i],SlugcatStats.Name.White), true);
            regionLabels.Add(regionName);
            OpCheckBox regionCheck = new OpCheckBox(new Configurable<bool>(true), new Vector2(10f, itemHeight - 32f -(70f * i)));
            regionCheck.OnValueChanged += RegionCheck_OnValueChanged;
            regionCheck.description = $"{i}.{array[i]}) Toggle weather this region on or off";
            regionCheckBoxes.Add(regionCheck);
            OpSimpleButton weatherSwitch = new OpSimpleButton(new Vector2(470f, itemHeight - 40f -(70f * i)), new Vector2(100f, 40f), "GLOBAL");
            weatherSwitch.OnClick += WeatherSwitch_OnClick;
            weatherSwitch.description = $"{i}.{array[i]}) Change this region's weather settings";
            regionButtons.Add(weatherSwitch);
            scrollBox.AddItems(rect, regionName, regionCheck, weatherSwitch);
        }

        #endregion

        
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
        supportRect.colorEdge = supportMode.Value ? new Color(0.2f, 1f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
        supportRect.colorFill = supportMode.Value ? new Color(0f, 0.5f, 0f) : new Color(0f, 0f, 0f);

        supportModeButton.text = supportMode.Value ? "DISABLE" : "ENABLE";
    }

    private void WeatherSwitch_OnClick(UIfocusable trigger)
    {
        int index = int.Parse(trigger.description.Split('.')[0]);
        string region = trigger.description.Split('.')[1].Split(')')[0];
        regionSettings[region]++;
        if (regionSettings[region] == 4) { regionSettings[region] = 0; }

        regionButtons[index].text = RegionModText(regionSettings[region]);
        regionButtons[index].colorEdge = RegionButtonColor(regionSettings[region]);
    }

    private Color RegionButtonColor(int val)
    {
        switch (val)
        {
            case 0:
                return new Color(0.5f,0.5f,0.5f);
            case 1:
                return new Color(0f,0.8f,1f);
            case 2:
                return new Color(1f,1f,1f);
            case 3:
                return new Color(0f, 1f, 0f);
        }
        return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
    }

    private string RegionModText(int val)
    {
        switch (val)
        {
            case 0:
                return "GLOBAL";
            case 1:
                return "RAIN";
            case 2:
                return "SNOW";
            case 3:
                return "CUSTOM";
        }
        return "ERROR";
    }

    private void RegionCheck_OnValueChanged(UIconfig config, string value, string oldValue)
    {
        int index = int.Parse(config.description.Split('.')[0]);
        if (regionCheckBoxes[index].value == "false")
        {
            regionRects[index].colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
            regionLabels[index].color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
            regionButtons[index].greyedOut = true;
        }
        else
        {
            regionRects[index].colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            regionLabels[index].color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            regionButtons[index].greyedOut = false;
        }
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

    }
}

//public class ForecastConfig : OptionInterface
//{
//    public static Vector2 topAnchor = new Vector2(30f, 480f);
//    public static Vector2 checkAnchor = new Vector2(topAnchor.x + 260f, topAnchor.y - 20f);
//    public OpRect topRect;
//    public OpRadioButtonGroup weatherType;
//    public OpRadioButton rainWeather;
//    public OpRadioButton snowWeather;
//    public OpLabel weatherTypeLabel;
//    public OpLabel rainIntensity;
//    public OpLabel rainSettings;
//    public OpSlider weatherIntensity;
//    public OpLabel weatherIntensityDescription;
//    public OpLabel rainLabel;
//    public OpLabel snowLabel;
//    public OpLabel dyn;
//    public OpLabel low;
//    public OpLabel med;
//    public OpLabel hig;
//    public OpLabel rainSettingsDescription;
//    public OpCheckBox lightningCheck;
//    public OpLabel intensitySliderLabel;
//    public OpLabel bgLabel;
//    public OpLabel directionSliderLabel;
//    public OpSlider weatherDirection;
//    public OpLabel rainChanceLabel;
//    public OpSlider rainChanceSlider;
//    public OpLabel lightningLabel;
//    public OpCheckBox paletteCheck;
//    public OpLabel paletteLabel;
//    public OpCheckBox muteCheck;
//    public OpLabel muteLabel;
//    public OpCheckBox effectCheck;
//    public OpLabel effectLabel;
//    public OpCheckBox waterCheck;
//    public OpLabel waterLabel;
//    public OpCheckBox bgOn;
//    public OpCheckBox dustCheck;
//    public OpLabel dustLabel;
//    public OpCheckBox blizzardCheck;
//    public OpLabel blizzardLabel;
//    public OpCheckBox decalCheck;
//    public OpLabel decalLabel;
//    public OpCheckBox strikeCheck;
//    public OpLabel strikeLabel;
//    public OpImage logo;
//    public OpImage logo2;
//    public OpLabel rainOption;
//    public OpSlider rainSlider;
//    public OpLabel[] regionLabelList;
//    public OpCheckBox[] regionChecks;
//    public OpLabel regionLabel;
//    public OpLabel regionDescription;
//    public OpLabel versionNumber;
//    public OpSimpleButton rainButton;
//    public OpSimpleButton snowButton;
//    public bool customRegionsEnabled;
//    public string[] regionList;
//    public OpSliderSubtle strikeDamage;
//    public OpLabel damageLabel;
//    public OpLabel snowWarning;

//    public ForecastConfig() : base(mod: Forecast.mod)
//    {
//    }

//    public override bool Configuable()
//    {
//        return true;
//    }

//    public override void Initialize()
//    {
//        regionList = RegionFinder.Generate().ToArray();

//        //Tabs
//        this.Tabs = new OpTab[1];
//        this.Tabs[0] = new OpTab("Weather");

//        //Weather Type
//        this.logo = new OpImage(new Vector2(270f, 540f), "logo");
//        this.logo2 = new OpImage(new Vector2(270f, 540f), "logo2");
//        this.weatherType = new OpRadioButtonGroup("Type", 0);
//        this.weatherTypeLabel = new OpLabel(new Vector2(30f, 570f), new Vector2(400f, 40f), "Weather Type", FLabelAlignment.Left, true);
//        this.rainWeather = new OpRadioButton(new Vector2(0f, 800f));
//        this.rainButton = new OpSimpleButton(new Vector2(30f, 540f), new Vector2(70f, 25f), "rainButton", "Rain");
//        this.snowWeather = new OpRadioButton(new Vector2(0f, 800f));
//        this.snowButton = new OpSimpleButton(new Vector2(110f, 540f), new Vector2(70f, 25f), "snowButton", "Snow");
//        this.versionNumber = new OpLabel(new Vector2(10f, -5f), new Vector2(0f, 0f), "Version: " + Forecast.mod.Version, FLabelAlignment.Left, false);
//        this.weatherType.SetButtons(new OpRadioButton[] { rainWeather, snowWeather });
//        this.Tabs[0].AddItems(rainButton, rainWeather, snowWeather, snowButton, weatherType, weatherTypeLabel, versionNumber);

//        //Weather Sliders
//        this.rainIntensity = new OpLabel(new Vector2(topAnchor.x, topAnchor.y), new Vector2(400f, 40f), "Rain Settings", FLabelAlignment.Left, true);
//        this.intensitySliderLabel = new OpLabel(new Vector2(topAnchor.x + 40f, topAnchor.y - 83f), new Vector2(400f, 40f), "Weather Progression", FLabelAlignment.Left, false);
//        this.weatherIntensity = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 60f), "weatherIntensity", new IntVector2(0, 3), 50f, false, 0);
//        this.rainSettingsDescription = new OpLabel(new Vector2(topAnchor.x, topAnchor.y - 20f), new Vector2(400f, 40f), "Enable or disable rain specific settings:", FLabelAlignment.Left, false);
//        this.directionSliderLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 143f), new Vector2(400f, 40f), "Weather Direction", FLabelAlignment.Left, false);
//        this.weatherDirection = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 120f), "weatherDirection", new IntVector2(0, 3), 50f, false, 0);
//        this.rainChanceLabel = new OpLabel(new Vector2(topAnchor.x + 47f, topAnchor.y - 203f), new Vector2(400f, 40f), "Weather Chance", FLabelAlignment.Left, false);
//        this.rainChanceSlider = new OpSlider(new Vector2(topAnchor.x + 25, topAnchor.y - 180f), "weatherChance", new IntVector2(0, 100), 1.5f, false, 100);
//        this.weatherIntensity.description = "Configure whether the intensity of the chosen weather increases as the cycle progresses or fix it to a certain intensity.";
//        this.weatherDirection.description = "Configure whether rain should fall towards a random or chosen direction.";
//        this.rainChanceSlider.description = "Configure whether the chosen weather will occur during a cycle.";
//        this.topRect = new OpRect(new Vector2(15f, 250f), new Vector2(570f, 270f), 0.1f);
//        this.Tabs[0].AddItems(rainIntensity, weatherIntensity, intensitySliderLabel, directionSliderLabel, weatherDirection, topRect, logo, logo2, rainChanceSlider, rainChanceLabel);

//        //Checkboxes
//        this.lightningCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 30f), "Lightning", false);
//        this.lightningLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, false);
//        this.paletteCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y + 10), "Palette", true);
//        this.paletteLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y +4f), new Vector2(400f, 40f), "Palette changes", FLabelAlignment.Left, false);
//        this.muteCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y + 10), "Mute", false);
//        this.muteLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y +4f), new Vector2(400f, 40f), "Mute interiors", FLabelAlignment.Left, false);
//        this.waterCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 30f), "Water", false);
//        this.waterLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Water ripples", FLabelAlignment.Left, false);
//        this.strikeCheck = new OpCheckBox(new Vector2(checkAnchor.x, checkAnchor.y - 70f), "Strike", true);
//        this.strikeLabel = new OpLabel(new Vector2(checkAnchor.x + 30f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Lightning Strikes", FLabelAlignment.Left, false);
//        this.strikeDamage = new OpSliderSubtle(new Vector2(checkAnchor.x + 10f, checkAnchor.y - 105f), "Damage", new IntVector2(0, 2), 110, false, 1);
//        this.damageLabel = new OpLabel(new Vector2(checkAnchor.x + 10f, checkAnchor.y - 123f), new Vector2(), "Damage Type: ", FLabelAlignment.Left, false);
//        this.bgOn = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 70f), "Background", true);
//        this.bgLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Background", FLabelAlignment.Left, false);
//        this.decalCheck = new OpCheckBox(new Vector2(checkAnchor.x+150f, checkAnchor.y - 30f), "Decals", true);
//        this.decalLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 36f), new Vector2(400f, 40f), "Surface decals", FLabelAlignment.Left, false);
//        this.dustCheck = new OpCheckBox(new Vector2(checkAnchor.x+150f, checkAnchor.y - 70f), "Dust", true);
//        this.dustLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 76f), new Vector2(400f, 40f), "Snow dust", FLabelAlignment.Left, false);
//        this.blizzardCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y - 110f), "Blizzard", true);
//        this.blizzardLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y - 116f), new Vector2(400f, 40f), "Blizzard", FLabelAlignment.Left, false);
//        this.effectCheck = new OpCheckBox(new Vector2(checkAnchor.x + 150f, checkAnchor.y + 10), "Effect", true);
//        this.effectLabel = new OpLabel(new Vector2(checkAnchor.x + 180f, checkAnchor.y + 4f), new Vector2(400f, 40f), "Effect Colors", FLabelAlignment.Left, false);
//        this.dustCheck.description = "Puffs of snow appear when landing on the ground.";
//        this.decalCheck.description = "Adds snowy decals to surfaces.";
//        this.lightningCheck.description = "Lightning will appear at higher weather intensities.";
//        this.paletteCheck.description = "The region will become darker with higher rain intensity.";
//        this.muteCheck.description = "Mute the sound effect added to interiors when its raining outside.";
//        this.waterCheck.description = "Rain drops can interact with water surfaces and cause ripples, may impact performance.";
//        this.bgOn.description = "Enable or disable collision with background elements, may impact performance.";
//        this.strikeDamage.description = "Adjust the damage type of Lightning Strikes";
//        this.strikeCheck.description = "When weather intensity is high enough, lightning strikes can occur.";
//        this.effectCheck.description = "Whitens things like plants and signs so they better match the snowy palette, can ruin some custom props";
//        this.blizzardCheck.description = "Replaces the end-of-cycle rain with a roaring blizzard and affects normal gameplay";
//        this.Tabs[0].AddItems(lightningLabel, lightningCheck, strikeCheck, strikeLabel, strikeDamage, damageLabel, rainSettingsDescription, paletteCheck, muteCheck, waterCheck, bgOn, paletteLabel, muteLabel, waterLabel, bgLabel, dustCheck,dustLabel,decalCheck,decalLabel, effectCheck, effectLabel, blizzardCheck,blizzardLabel);

//        //Particle Limit
//        this.rainOption = new OpLabel(new Vector2(topAnchor.x + 366f, topAnchor.y - 223f), new Vector2(400f, 40f), "Particle Limit", FLabelAlignment.Left, false);
//        this.rainSlider = new OpSlider(new Vector2(topAnchor.x + 275f, topAnchor.y - 200f), "rainAmount", new IntVector2(10, 80), 3.3f, false, 50);
//        this.Tabs[0].AddItems(rainSlider, rainOption);

//        //Regions
//        if (regionList != null)
//        {
//            this.regionLabelList = new OpLabel[regionList.Length];
//            this.regionChecks = new OpCheckBox[regionList.Length];
//            this.regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
//            this.regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable weather on a per-region basis.", FLabelAlignment.Left, false);
//            this.Tabs[0].AddItems(regionLabel, regionDescription);
//            for (int i = 0; i < regionList.Length; i++)
//            {
//                if (i < 10)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(30f + (55f * i), 150f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(60f + (55f * i), 142f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 10 && i < 20)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-520f + (55f * i), 105f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-490f + (55f * i), 97f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 20 && i < 30)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-1070f + (55f * i), 60f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-1040f + (55f * i), 52f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                else if (i >= 30)
//                {
//                    regionChecks[i] = new OpCheckBox(new Vector2(-1650f + (55f * i), 15f), regionList[i], true);
//                    regionLabelList[i] = new OpLabel(new Vector2(-1620f + (55f * i), 7f), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, false);
//                }
//                this.Tabs[0].AddItems(regionLabelList[i], regionChecks[i]);
//                if (regionList[i] == "SS")
//                {
//                    regionChecks[i].valueBool = false;
//                }
//            }
//        }
//        Forecast.configLoaded = true;
//    }
//    public override void Signal(UItrigger trigger, string signal)
//    {
//        base.Signal(trigger, signal);
//        if (signal == "rainButton")
//        {
//            this.rainWeather.valueBool = true;
//        }
//        if (signal == "snowButton")
//        {
//            this.snowWeather.valueBool = true;
//        }
//    }
//    public override void Update(float dt)
//    {
//        base.Update(dt);
//        //Toggle between rain and snow mode
//        if (this.rainWeather.valueBool)
//        {
//            rainIntensity.text = "Rain Settings";
//            rainSettingsDescription.text = "Enable or disable rain specific settings:";
//            paletteCheck.description = "The region will become darker with higher rain intensity.";
//            logo.Show();
//            logo2.Hide();
//            //Hide rain checks
//            muteCheck.Show();
//            waterCheck.Show();
//            bgOn.Show();
//            //Show rain check labels
//            bgLabel.Show();
//            muteLabel.Show();
//            waterLabel.Show();
//            //Hide snow checks
//            decalCheck.Hide();
//            dustCheck.Hide();
//            effectCheck.Hide();
//            blizzardCheck.Hide();
//            //Hide snow labels
//            decalLabel.Hide();
//            dustLabel.Hide();
//            effectLabel.Hide();
//            blizzardLabel.Hide();
//        }
//        else
//        {
//            rainIntensity.text = "Snow Settings";
//            rainSettingsDescription.text = "Enable or disable snow specific settings:";
//            paletteCheck.description = "A snowy palette will be overlayed onto the current palette.";
//            logo.Hide();
//            logo2.Show();
//            //Disable rain checks
//            muteCheck.valueBool = false;
//            waterCheck.valueBool = false;
//            bgOn.valueBool = false;
//            //Hide rain checks
//            muteCheck.Hide();
//            waterCheck.Hide();
//            bgOn.Hide();
//            //Hide rain check labels
//            bgLabel.Hide();
//            muteLabel.Hide();
//            waterLabel.Hide();
//            //Hide snow checks
//            decalCheck.Show();
//            dustCheck.Show();
//            effectCheck.Show();
//            blizzardCheck.Show();
//            //Hide snow labels
//            decalLabel.Show();
//            dustLabel.Show();
//            effectLabel.Show();
//            blizzardLabel.Show();
//        }
//        if(lightningCheck.valueBool == false)
//        {
//            strikeCheck.valueBool = false;
//            strikeCheck.greyedOut = true;
//            strikeCheck.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            strikeLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//        }
//        else
//        {
//            strikeCheck.greyedOut = false;
//            strikeCheck.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            strikeLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//        }
//        if (lightningCheck.valueBool == false || strikeCheck.valueBool == false)
//        {
//            strikeDamage.greyedOut = true;
//            strikeDamage.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            strikeDamage.colorLine = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//            damageLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb;
//        }
//        else
//        {
//            strikeDamage.greyedOut = false;
//            strikeDamage.colorEdge = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            strikeDamage.colorLine = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//            damageLabel.color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb;
//        }
//        if (this.strikeDamage != null)
//        {
//            switch (this.strikeDamage.valueInt)
//            {
//                case 0:
//                    this.damageLabel.text = "Damage Type: None";
//                    break;
//                case 1:
//                    this.damageLabel.text = "Damage Type: Stun";
//                    break;
//                case 2:
//                    this.damageLabel.text = "Damage Type: Lethal";
//                    break;
//            }
//        }
//        //Intensity Slider
//        switch (weatherIntensity.value)
//        {
//            case "0":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Dynamic";
//                break;
//            case "1":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Low";
//                break;
//            case "2":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "Medium";
//                break;
//            case "3":
//                (weatherIntensity.subObjects[1] as Menu.MenuLabel).text = "High";
//                break;
//        }
//        //Direction Slider
//        switch (weatherDirection.value)
//        {
//            case "0":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Random";
//                break;
//            case "1":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Left";
//                break;
//            case "2":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Center";
//                break;
//            case "3":
//                (weatherDirection.subObjects[1] as Menu.MenuLabel).text = "Right";
//                break;
//        }
//    }
//    public override void ConfigOnChange()
//    {
//        Forecast.rainRegions = new List<string>();
//        for (int i = 0; i < regionList.Length; i++)
//        {
//            if (config[regionList[i]] == "true")
//            {
//                Forecast.rainRegions.Add(regionList[i]);
//            }
//        }
//        if (config["Type"] == "0")
//        {
//            Forecast.snow = false;
//        }
//        else
//        {
//            Forecast.snow = true;
//        }
//        if (config["Palette"] == "false")
//        {
//            Forecast.paletteChange = false;
//        }
//        else
//        {
//            Forecast.paletteChange = true;
//        }
//        if(config["Effect"] == "false")
//        {
//            Forecast.effectColors = false;
//        }
//        else
//        {
//            Forecast.effectColors = true;
//        }
//        if(config["Blizzard"] == "false")
//        {
//            Forecast.blizzard = false;
//        }
//        else
//        {
//            Forecast.blizzard = true;
//        }
//        if (config["Mute"] == "false")
//        {
//            Forecast.interiorRain = false;
//        }
//        else
//        {
//            Forecast.interiorRain = true;
//        }
//        if (config["Water"] == "false")
//        {
//            Forecast.water = false;
//        }
//        else
//        {
//            Forecast.water = true;
//        }
//        if (config["Background"] == "false")
//        {
//            Forecast.bg = false;
//        }
//        else
//        {
//            Forecast.bg = true;
//        }
//        if (config["Lightning"] == "false")
//        {
//            Forecast.lightning = false;
//        }
//        else
//        {
//            Forecast.lightning = true;
//        }
//        if (config["Strike"] == "false")
//        {
//            Forecast.strike = false;
//        }
//        else
//        {
//            Forecast.strike = true;
//        }
//        if (config["Decals"] == "false")
//        {
//            Forecast.decals = false;
//        }
//        else
//        {
//            Forecast.decals = true;
//        }
//        if (config["Dust"] == "false")
//        {
//            Forecast.dust = false;
//        }
//        else
//        {
//            Forecast.dust = true;
//        }
//        if (config["weatherIntensity"] == "0")
//        {
//            Forecast.intensity = 0;
//            Forecast.dynamic = true;
//        }
//        if (config["weatherIntensity"] == "1")
//        {
//            Forecast.intensity = 1;
//            Forecast.dynamic = false;
//        }
//        if (config["weatherIntensity"] == "2")
//        {
//            Forecast.intensity = 2;
//            Forecast.dynamic = false;
//        }
//        if (config["weatherIntensity"] == "3")
//        {
//            Forecast.intensity = 3;
//            Forecast.dynamic = false;
//        }
//        Forecast.rainAmount = int.Parse(config["rainAmount"]);
//        Forecast.direction = int.Parse(config["weatherDirection"]);
//        Forecast.rainChance = int.Parse(config["weatherChance"]);
//        Forecast.strikeDamage = int.Parse(config["Damage"]);
//    }
//}
