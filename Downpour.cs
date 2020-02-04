using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using OptionalUI;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using RWCustom;
using Menu;

public class RainScript : MonoBehaviour
{
    public static Downpour mod;
    public void Initialize()
    {
        RainFall.Patch();
        RainPalette.Patch();
    }
    public void Update()
    {

    }
}
public class Downpour : PartialityMod
{
    public Downpour()
    {
        this.ModID = "Downpour";
        this.Version = "Beta";
        this.author = "LeeMoriya";
    }

    public static RainScript script;
    public static bool paletteChange = true;
    public static bool lightning = true;
    public static bool dynamic = true;
    public static int intensity = 0;
    public static bool rainbow = false;
    public static bool configLoaded = false;
    public static bool debug = true;
    public static bool snow = false;
    public static List<string> rainRegions = new List<string>();
    public static int rainAmount;

    public override void OnEnable()
    {
        base.OnEnable();
        RainScript.mod = this;
        GameObject go = new GameObject();
        script = go.AddComponent<RainScript>();
        script.Initialize();
    }
    public OptionalUI.OptionInterface LoadOI()
    {
        if (oiType == null)
            MakeOIType();

        return (OptionalUI.OptionInterface)Activator.CreateInstance(oiType, new object[] { this });
    }

    public static Type oiType = null;
    private void MakeOIType()
    {
        Debug.Log("Loading DownpourOptions...");
        AssemblyName name = new AssemblyName("DownpourOI");
        AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder mb = ab.DefineDynamicModule(name.Name);
        TypeBuilder tb = mb.DefineType("DownpourOptions", TypeAttributes.Class, typeof(OptionalUI.OptionInterface));

        ConstructorBuilder cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(Partiality.Modloader.PartialityMod) });
        ILGenerator ctorILG = cb.GetILGenerator();
        ctorILG.Emit(OpCodes.Ldarg_0);
        ctorILG.Emit(OpCodes.Ldarg_1);
        ctorILG.Emit(OpCodes.Call, typeof(OptionalUI.OptionInterface).GetConstructor(new Type[] { typeof(Partiality.Modloader.PartialityMod) }));
        ctorILG.Emit(OpCodes.Ret);

        MethodBuilder initmb = tb.DefineMethod("Initialize", MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig);
        ILGenerator initmbILG = initmb.GetILGenerator();
        initmbILG.Emit(OpCodes.Ldarg_0);
        initmbILG.Emit(OpCodes.Call, typeof(OptionalUI.OptionInterface).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        initmbILG.Emit(OpCodes.Ldarg_0);
        initmbILG.Emit(OpCodes.Call, typeof(DOProxy).GetMethod("Initialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
        initmbILG.Emit(OpCodes.Ret);

        MethodBuilder ccmb = tb.DefineMethod("ConfigOnChange", MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.HideBySig);
        ILGenerator ccmbILG = ccmb.GetILGenerator();
        ccmbILG.Emit(OpCodes.Ldarg_0);
        ccmbILG.Emit(OpCodes.Call, typeof(OptionalUI.OptionInterface).GetMethod("ConfigOnChange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        ccmbILG.Emit(OpCodes.Ldarg_0);
        ccmbILG.Emit(OpCodes.Call, typeof(DOProxy).GetMethod("ConfigOnChange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
        ccmbILG.Emit(OpCodes.Ret);

        oiType = tb.CreateType();
        configLoaded = true;
        Debug.Log("Loaded DownpourOptions");
    }
}

public class DOProxy
{
    //Setup ConfigMachine GUI
    public static void Initialize(OptionalUI.OptionInterface self)
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
        //List<string> regionList = Menu.FastTravelScreen.GetRegionOrder();
        self.Tabs = new OpTab[1];
        self.Tabs[0] = new OpTab("Options");
        //Rain
        OptionalUI.OpLabel rainIntensity = new OpLabel(new Vector2(30f, 560f), new Vector2(400f, 40f), "Rain Intensity", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainIntensityDescription = new OpLabel(new Vector2(30f, 537f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButtonGroup intensityGroup = new OpRadioButtonGroup("Setting", 0);
        OptionalUI.OpRadioButton intensityDynamic = new OpRadioButton(new Vector2(30f, 510f));
        intensityDynamic.description = "Intensity of the rain is randomly determined and affected by karma level, there can also be no rain at all.";
        OptionalUI.OpLabel dynamicLabel = new OpLabel(new Vector2(60f, 503f), new Vector2(400f, 40f), "Dynamic", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityLow = new OpRadioButton(new Vector2(130f, 510f));
        intensityLow.description = "Intensity of the rain will be fixed to Low intensity.";
        OptionalUI.OpLabel lowLabel = new OpLabel(new Vector2(160f, 503f), new Vector2(400f, 40f), "Low", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityMed = new OpRadioButton(new Vector2(210f, 510f));
        intensityMed.description = "Intensity of the rain will be fixed to Medium intensity.";
        OptionalUI.OpLabel medLabel = new OpLabel(new Vector2(240f, 503f), new Vector2(400f, 40f), "Med", FLabelAlignment.Left, false);
        OptionalUI.OpRadioButton intensityHigh = new OpRadioButton(new Vector2(290f, 510f));
        intensityHigh.description = "Intensity of the rain will be fixed to High intensity.";
        OptionalUI.OpLabel highLabel = new OpLabel(new Vector2(320f, 503f), new Vector2(400f, 40f), "High", FLabelAlignment.Left, false);
        intensityGroup.SetButtons(new OpRadioButton[] { intensityDynamic, intensityLow, intensityMed, intensityHigh });
        self.Tabs[0].AddItems(intensityGroup,highLabel,intensityDynamic,intensityHigh,intensityMed,intensityLow,rainIntensity,rainIntensityDescription,medLabel,lowLabel,dynamicLabel);
        //Lightning
        OptionalUI.OpLabel environmentOption = new OpLabel(new Vector2(30f, 460f), new Vector2(400f, 40f), "Environment", FLabelAlignment.Left, true);
        OptionalUI.OpLabel lightningOptionDescription = new OpLabel(new Vector2(30f, 437f), new Vector2(400f, 40f), "Configure which effects are added to the environment during heavy rain.", FLabelAlignment.Left, false);
        OptionalUI.OpCheckBox lightningCheck = new OpCheckBox(new Vector2(30f, 415f), "Lightning", true);
        lightningCheck.description = "Lightning will appear in regions when rain intensity is high enough.";
        OptionalUI.OpLabel lightningLabel = new OpLabel(new Vector2(60f, 408f), new Vector2(400f, 40f), "Lightning storms", FLabelAlignment.Left, false);
        self.Tabs[0].AddItems(lightningLabel, lightningCheck, lightningOptionDescription,environmentOption);
        //Palette
        OptionalUI.OpCheckBox paletteCheck = new OpCheckBox(new Vector2(30f, 385f), "Palette", true);
        paletteCheck.description = "The region will become darker with higher rain intensity.";
        OptionalUI.OpLabel paletteLabel = new OpLabel(new Vector2(60f, 378f), new Vector2(400f, 40f), "Regions become darker", FLabelAlignment.Left, false);
        self.Tabs[0].AddItems(paletteLabel,paletteCheck);
        //Rainbow
        OptionalUI.OpCheckBox rainbowOn = new OpCheckBox(new Vector2(30f, 355f), "Rainbow", false);
        rainbowOn.description = "Raindrop colors will be randomized.";
        OptionalUI.OpLabel onrainbowLabel = new OpLabel(new Vector2(60f, 348f), new Vector2(400f, 40f), "Taste the rainbow", FLabelAlignment.Left, false);
        self.Tabs[0].AddItems(onrainbowLabel,rainbowOn);
        //Snow
        OptionalUI.OpCheckBox snowOn = new OpCheckBox(new Vector2(230f, 415f), "Snow", false);
        rainbowOn.description = "Replace rain with snow.";
        OptionalUI.OpLabel snowLabel = new OpLabel(new Vector2(260f, 408f), new Vector2(400f, 40f), "Snow", FLabelAlignment.Left, false);
        self.Tabs[0].AddItems(snowOn, snowLabel);
        //Raindrops
        OptionalUI.OpLabel rainOption = new OpLabel(new Vector2(30f, 308f), new Vector2(400f, 40f), "Raindrops", FLabelAlignment.Left, true);
        OptionalUI.OpLabel rainOptionDescription = new OpLabel(new Vector2(30f, 288f), new Vector2(400f, 40f), "Configure the maximum amount of raindrops that can be spawned each frame.", FLabelAlignment.Left, false);
        OptionalUI.OpLabel rainOptionWarning = new OpLabel(new Vector2(30f, 273f), new Vector2(400f, 40f), "Warning: You may experience significant framedrops if this slider is set too high.", FLabelAlignment.Left, false);
        OptionalUI.OpSlider rainSlider = new OpSlider(new Vector2(30f, 240f), "rainAmount", new IntVector2(10, 90), 3.5f, false, 60);
        self.Tabs[0].AddItems(rainSlider,rainOption,rainOptionDescription, rainOptionWarning);
        //Regions 
        if (regionList != null)
        {
            OptionalUI.OpLabel[] regionLabelList = new OpLabel[regionList.Length];
            OptionalUI.OpCheckBox[] regionChecks = new OpCheckBox[regionList.Length];
            OptionalUI.OpLabel regionLabel = new OpLabel(new Vector2(30f, 200f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
            OptionalUI.OpLabel regionDescription = new OpLabel(new Vector2(30f, 175f), new Vector2(400f, 40f), "Enable and Disable rainfall on a per-region basis.", FLabelAlignment.Left, false);
            self.Tabs[0].AddItems(regionLabel,regionDescription);
            for (int i = 0; i < regionList.Length; i++)
            {
                if (i < 6)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(30f + (95f * i), 150f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(60f + (95f * i), 145f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >=6 && i < 12)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-540f + (95f * i), 105f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-510f + (95f * i), 100f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >= 12 && i < 18)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-1110f + (95f * i), 60f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-1080f + (95f * i), 55f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                else if (i >= 18)
                {
                    regionChecks[i] = new OpCheckBox(new Vector2(-1680f + (95f * i), 15f), regionList[i], true);
                    regionLabelList[i] = new OpLabel(new Vector2(-1650f + (95f * i), 10f), new Vector2(400f, 40f), "-" + regionList[i], FLabelAlignment.Left, true);
                }
                self.Tabs[0].AddItems(regionLabelList[i], regionChecks[i]);
                if (regionList[i] == "UW" || regionList[i] == "SB" || regionList[i] == "SS")
                {
                    regionChecks[i].valueBool = false;
                }
            }
        }
    }

    // Apply changes to the mod
    public static void ConfigOnChange(OptionalUI.OptionInterface self)
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
        //List<string> regionList = Menu.FastTravelScreen.GetRegionOrder();
        Downpour.rainRegions = new List<string>();
        for (int i = 0; i < regionList.Length; i++)
        {
            if (OptionalUI.OptionInterface.config[regionList[i]] == "true")
            {
                Downpour.rainRegions.Add(regionList[i]);
            }
        }
        Downpour.rainAmount = int.Parse(OptionalUI.OptionInterface.config["rainAmount"]);
        if (OptionalUI.OptionInterface.config["Palette"] == "false")
        {
            Downpour.paletteChange = false;
        }
        else
        {
            Downpour.paletteChange = true;
        }
        if (OptionalUI.OptionInterface.config["Snow"] == "false")
        {
            Downpour.snow = false;
        }
        else
        {
            Downpour.snow = true;
        }
        if (OptionalUI.OptionInterface.config["Lightning"] == "false")
        {
            Downpour.lightning = false;
        }
        else
        {
            Downpour.lightning = true;
        }
        if (OptionalUI.OptionInterface.config["Rainbow"] == "false")
        {
            Downpour.rainbow = false;
        }
        else
        {
            Downpour.rainbow = true;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "0")
        {
            Downpour.intensity = 0;
            Downpour.dynamic = true;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "1")
        {
            Downpour.intensity = 1;
            Downpour.dynamic = false;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "2")
        {
            Downpour.intensity = 2;
            Downpour.dynamic = false;
        }
        if (OptionalUI.OptionInterface.config["Setting"] == "3")
        {
            Downpour.intensity = 3;
            Downpour.dynamic = false;
        }
    }
}