using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using OptionalUI;
using System.Reflection;
using System.Reflection.Emit;

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
        this.Version = "1.0";
        this.author = "LeeMoriya";
    }

    public static RainScript script;
    public static bool paletteChange = true;
    public static bool lightning = true;
    public static bool dynamic = true;
    public static int intensity = 0;

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
    public static void Initialize(OptionalUI.OptionInterface self)
    {
        self.Tabs = new OpTab[1];
        self.Tabs[0] = new OpTab("Options");
        //Rain
        OptionalUI.OpLabel rainIntensity = new OpLabel(new Vector2(30f, 550f), new Vector2(400f, 40f), "Rain Intensity", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(rainIntensity);
        OptionalUI.OpLabel rainIntensityDescription = new OpLabel(new Vector2(30f, 520f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(rainIntensityDescription);
        OptionalUI.OpRadioButtonGroup intensityGroup = new OpRadioButtonGroup("Setting", 0);
        OptionalUI.OpRadioButtonGroup lightningGroup = new OpRadioButtonGroup("Lightning", 1);
        OptionalUI.OpRadioButtonGroup paletteGroup = new OpRadioButtonGroup("Palette", 1);
        OptionalUI.OpRadioButton intensityDynamic = new OpRadioButton(new Vector2(30f, 490f));
        intensityDynamic.description = "Intensity of the rain is randomly determined and affected by karma level.";
        self.Tabs[0].AddItem(intensityDynamic);
        OptionalUI.OpLabel dynamicLabel = new OpLabel(new Vector2(60f, 485f), new Vector2(400f, 40f), "Dynamic", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(dynamicLabel);
        OptionalUI.OpRadioButton intensityLow = new OpRadioButton(new Vector2(30f, 460f));
        intensityLow.description = "Intensity of the rain will be fixed to Low intensity.";
        self.Tabs[0].AddItem(intensityLow);
        OptionalUI.OpLabel lowLabel = new OpLabel(new Vector2(60f, 455f), new Vector2(400f, 40f), "Low", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(lowLabel);
        OptionalUI.OpRadioButton intensityMed = new OpRadioButton(new Vector2(30f, 430f));
        intensityMed.description = "Intensity of the rain will be fixed to Medium intensity.";
        self.Tabs[0].AddItem(intensityMed);
        OptionalUI.OpLabel medLabel = new OpLabel(new Vector2(60f, 425f), new Vector2(400f, 40f), "Medium", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(medLabel);
        OptionalUI.OpRadioButton intensityHigh = new OpRadioButton(new Vector2(30f, 400f));
        intensityHigh.description = "Intensity of the rain will be fixed to High intensity.";
        self.Tabs[0].AddItem(intensityHigh);
        OptionalUI.OpLabel highLabel = new OpLabel(new Vector2(60f, 395f), new Vector2(400f, 40f), "High", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(highLabel);
        intensityGroup.SetButtons(new OpRadioButton[] { intensityDynamic, intensityLow, intensityMed, intensityHigh });
        self.Tabs[0].AddItem(intensityGroup);
        //Lightning
        OptionalUI.OpLabel lightningOption = new OpLabel(new Vector2(30f, 350f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(lightningOption);
        OptionalUI.OpLabel lightningOptionDescription = new OpLabel(new Vector2(30f, 320f), new Vector2(400f, 40f), "Determines whether a lightning effect is added during intense rain.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(lightningOptionDescription);
        OptionalUI.OpRadioButton lightningOff = new OpRadioButton(new Vector2(30f, 300f));
        lightningOff.description = "Lightning effects will not be added during intense rain.";
        self.Tabs[0].AddItem(lightningOff);
        OptionalUI.OpLabel offLabel = new OpLabel(new Vector2(60f, 295f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(offLabel);
        OptionalUI.OpRadioButton lightningOn = new OpRadioButton(new Vector2(30f, 270f));
        lightningOn.description = "Lightning effects will be added during intense rain.";
        self.Tabs[0].AddItem(lightningOn);
        OptionalUI.OpLabel onLabel = new OpLabel(new Vector2(60f, 265f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(onLabel);
        lightningGroup.SetButtons(new OpRadioButton[] { lightningOff, lightningOn });
        self.Tabs[0].AddItem(lightningGroup);
        //Palette
        OptionalUI.OpLabel paletteOption = new OpLabel(new Vector2(30f, 210f), new Vector2(400f, 40f), "Darkening", FLabelAlignment.Left, true);
        self.Tabs[0].AddItem(paletteOption);
        OptionalUI.OpLabel paletteOptionDescription = new OpLabel(new Vector2(30f, 180f), new Vector2(400f, 40f), "Change whether the rain intensity will darken the color palette.", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(paletteOptionDescription);
        OptionalUI.OpRadioButton paletteOff = new OpRadioButton(new Vector2(30f, 160f));
        paletteOff.description = "The region will not become darker with higher rain intensity.";
        self.Tabs[0].AddItem(paletteOff);
        OptionalUI.OpLabel offPaletteLabel = new OpLabel(new Vector2(60f, 155f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(offPaletteLabel);
        OptionalUI.OpRadioButton paletteOn = new OpRadioButton(new Vector2(30f, 130f));
        paletteOn.description = "The region will become darker with higher rain intensity";
        self.Tabs[0].AddItem(paletteOn);
        OptionalUI.OpLabel onPaletteLabel = new OpLabel(new Vector2(60f, 125f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        self.Tabs[0].AddItem(onPaletteLabel);
        paletteGroup.SetButtons(new OpRadioButton[] { paletteOff, paletteOn });
        self.Tabs[0].AddItem(paletteGroup);
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