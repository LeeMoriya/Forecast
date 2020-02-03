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

    private Type oiType = null;
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
        Debug.Log("Loaded DownpourOptions");
    }
}

public class DOProxy
{
    //Setup ConfigMachine GUI
    public static void Initialize(OptionalUI.OptionInterface self)
    {
        string[] regionList = File.ReadAllLines(Custom.RootFolderDirectory() + "/World/Regions/regions.txt");
        self.Tabs = new OpTab[2];
        self.Tabs[0] = new OpTab("Options");
        self.Tabs[1] = new OpTab("Regions");
        //Rain
        OptionalUI.OpLabel rainIntensity = new OpLabel(new Vector2(30f, 560f), new Vector2(400f, 40f), "Rain Intensity", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(rainIntensity);
        OptionalUI.OpLabel rainIntensityDescription = new OpLabel(new Vector2(30f, 530f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(rainIntensityDescription);
        OptionalUI.OpRadioButtonGroup intensityGroup = new OpRadioButtonGroup("Setting", 0);
        OptionalUI.OpRadioButtonGroup lightningGroup = new OpRadioButtonGroup("Lightning", 1);
        OptionalUI.OpRadioButtonGroup paletteGroup = new OpRadioButtonGroup("Palette", 1);
        OptionalUI.OpRadioButtonGroup rainbowGroup = new OpRadioButtonGroup("Rainbow", 0);
        OptionalUI.OpRadioButton intensityDynamic = new OpRadioButton(new Vector2(30f, 500f));
        intensityDynamic.description = "Intensity of the rain is randomly determined and affected by karma level, there can also be no rain at all.";
        self.Tabs[0].AddItem(intensityDynamic);
        OptionalUI.OpLabel dynamicLabel = new OpLabel(new Vector2(60f, 495f), new Vector2(400f, 40f), "Dynamic", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(dynamicLabel);
        OptionalUI.OpRadioButton intensityLow = new OpRadioButton(new Vector2(30f, 470f));
        intensityLow.description = "Intensity of the rain will be fixed to Low intensity.";
        self.Tabs[0].AddItem(intensityLow);
        OptionalUI.OpLabel lowLabel = new OpLabel(new Vector2(60f, 465f), new Vector2(400f, 40f), "Low", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(lowLabel);
        OptionalUI.OpRadioButton intensityMed = new OpRadioButton(new Vector2(30f, 440f));
        intensityMed.description = "Intensity of the rain will be fixed to Medium intensity.";
        self.Tabs[0].AddItem(intensityMed);
        OptionalUI.OpLabel medLabel = new OpLabel(new Vector2(60f, 435f), new Vector2(400f, 40f), "Medium", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(medLabel);
        OptionalUI.OpRadioButton intensityHigh = new OpRadioButton(new Vector2(30f, 410f));
        intensityHigh.description = "Intensity of the rain will be fixed to High intensity.";
        self.Tabs[0].AddItem(intensityHigh);
        OptionalUI.OpLabel highLabel = new OpLabel(new Vector2(60f, 405f), new Vector2(400f, 40f), "High", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(highLabel);
        intensityGroup.SetButtons(new OpRadioButton[] { intensityDynamic, intensityLow, intensityMed, intensityHigh });
        self.Tabs[0].AddItem(intensityGroup);
        //Lightning
        OptionalUI.OpLabel lightningOption = new OpLabel(new Vector2(30f, 360f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(lightningOption);
        OptionalUI.OpLabel lightningOptionDescription = new OpLabel(new Vector2(30f, 330f), new Vector2(400f, 40f), "Determines whether a lightning effect is added during intense rain.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(lightningOptionDescription);
        OptionalUI.OpRadioButton lightningOff = new OpRadioButton(new Vector2(30f, 310f));
        lightningOff.description = "Lightning effects will not be added during intense rain.";
        self.Tabs[0].AddItem(lightningOff);
        OptionalUI.OpLabel offLabel = new OpLabel(new Vector2(60f, 305f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(offLabel);
        OptionalUI.OpRadioButton lightningOn = new OpRadioButton(new Vector2(100f, 310f));
        lightningOn.description = "Lightning effects will be added during intense rain.";
        self.Tabs[0].AddItem(lightningOn);
        OptionalUI.OpLabel onLabel = new OpLabel(new Vector2(130f, 305f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(onLabel);
        lightningGroup.SetButtons(new OpRadioButton[] { lightningOff, lightningOn });
        self.Tabs[0].AddItem(lightningGroup);
        //Palette
        OptionalUI.OpLabel paletteOption = new OpLabel(new Vector2(30f, 220f), new Vector2(400f, 40f), "Darkening", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(paletteOption);
        OptionalUI.OpLabel paletteOptionDescription = new OpLabel(new Vector2(30f, 190f), new Vector2(400f, 40f), "Change whether the rain intensity will darken the color palette.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(paletteOptionDescription);
        OptionalUI.OpRadioButton paletteOff = new OpRadioButton(new Vector2(30f, 170f));
        paletteOff.description = "The region will not become darker with higher rain intensity.";
        self.Tabs[0].AddItem(paletteOff);
        OptionalUI.OpLabel offPaletteLabel = new OpLabel(new Vector2(60f, 165f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(offPaletteLabel);
        OptionalUI.OpRadioButton paletteOn = new OpRadioButton(new Vector2(100f, 170f));
        paletteOn.description = "The region will become darker with higher rain intensity";
        self.Tabs[0].AddItem(paletteOn);
        OptionalUI.OpLabel onPaletteLabel = new OpLabel(new Vector2(130f, 165f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(onPaletteLabel);
        paletteGroup.SetButtons(new OpRadioButton[] { paletteOff, paletteOn });
        self.Tabs[0].AddItem(paletteGroup);
        //Rainbow
        OptionalUI.OpLabel rainbowOption = new OpLabel(new Vector2(30f, 80f), new Vector2(400f, 40f), "Skittle Mode", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(rainbowOption);
        OptionalUI.OpLabel rainbowOptionDescription = new OpLabel(new Vector2(30f, 50f), new Vector2(400f, 40f), "Randomizes raindrop color", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(rainbowOptionDescription);
        OptionalUI.OpRadioButton rainbowOff = new OpRadioButton(new Vector2(30f, 30f));
        rainbowOff.description = "Raindrop colors will match the region palette";
        self.Tabs[0].AddItem(rainbowOff);
        OptionalUI.OpLabel offrainbowLabel = new OpLabel(new Vector2(60f, 25f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(offrainbowLabel);
        OptionalUI.OpRadioButton rainbowOn = new OpRadioButton(new Vector2(100f, 30f));
        rainbowOn.description = "Raindrop colors will be randomized";
        self.Tabs[0].AddItem(rainbowOn);
        OptionalUI.OpLabel onrainbowLabel = new OpLabel(new Vector2(130f, 25f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(onrainbowLabel);
        rainbowGroup.SetButtons(new OpRadioButton[] { rainbowOff, rainbowOn });
        self.Tabs[0].AddItem(rainbowGroup);
        //---End of First Tab---

        //---Regions Tab---
        if(regionList != null)
        {
            OptionalUI.OpRadioButtonGroup[] regionGroup = new OpRadioButtonGroup[regionList.Length];
            OptionalUI.OpRadioButton[] regionOnButtons = new OpRadioButton[regionList.Length];
            OptionalUI.OpRadioButton[] regionOffButtons = new OpRadioButton[regionList.Length];
            OptionalUI.OpLabel[] regionOffLabels = new OpLabel[regionList.Length];
            OptionalUI.OpLabel[] regionOnLabels = new OpLabel[regionList.Length];
            OptionalUI.OpLabel[] regionLabelList = new OpLabel[regionList.Length];
            OptionalUI.OpLabel regionLabel = new OpLabel(new Vector2(30f, 560f), new Vector2(400f, 40f), "Region Settings", FLabelAlignment.Left, true);
            OptionalUI.OpLabel regionDescription  = new OpLabel(new Vector2(30f, 535f), new Vector2(400f, 40f), "Enable and Disable rainfall on a per-region basis.", FLabelAlignment.Left, false);
            self.Tabs[1].AddItem(regionLabel);
            self.Tabs[1].AddItem(regionDescription);

            for (int i = 0; i < regionList.Length; i++)
            {
                regionGroup[i] = new OpRadioButtonGroup(regionList[i], 1);
                if (100f * i < 700f)
                {
                    regionLabelList[i] = new OpLabel(new Vector2(30f, 490f - (75f * i)), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, true);
                    regionOffButtons[i] = new OpRadioButton(new Vector2(30f, 470f - (75f * i)));
                    regionOnButtons[i] = new OpRadioButton(new Vector2(100f, 470f - (75f * i)));
                    regionOffLabels[i] = new OpLabel(new Vector2(60f, 460f - (75f * i)), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
                    regionOnLabels[i] = new OpLabel(new Vector2(130f, 460f - (75f * i)), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
                }
                else
                {
                    regionLabelList[i] = new OpLabel(new Vector2(250f, 1015f - (75f * i)), new Vector2(400f, 40f), regionList[i], FLabelAlignment.Left, true);
                    regionOffButtons[i] = new OpRadioButton(new Vector2(250f, 995f - (75f * i)));
                    regionOnButtons[i] = new OpRadioButton(new Vector2(320f, 995f - (75f * i)));
                    regionOffLabels[i] = new OpLabel(new Vector2(280f, 985f - (75f * i)), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
                    regionOnLabels[i] = new OpLabel(new Vector2(350f, 985f - (75f * i)), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
                }
                regionGroup[i].SetButtons(new OpRadioButton[] { regionOffButtons[i], regionOnButtons[i] });
                self.Tabs[1].AddItem(regionLabelList[i]);
                self.Tabs[1].AddItem(regionOffButtons[i]);
                self.Tabs[1].AddItem(regionOnButtons[i]);
                self.Tabs[1].AddItem(regionOffLabels[i]);
                self.Tabs[1].AddItem(regionOnLabels[i]);
                self.Tabs[1].AddItem(regionGroup[i]);
            }
        }
    }

    // Apply changes to the mod
    public static void ConfigOnChange(OptionalUI.OptionInterface self)
    {
        if (OptionalUI.OptionInterface.config["Palette"] == "0")
        {
            Downpour.paletteChange = false;
        }
        else
        {
            Downpour.paletteChange = true;
        }
        if (OptionalUI.OptionInterface.config["Lightning"] == "0")
        {
            Downpour.lightning = false;
        }
        else
        {
            Downpour.lightning = true;
        }
        if (OptionalUI.OptionInterface.config["Rainbow"] == "0")
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