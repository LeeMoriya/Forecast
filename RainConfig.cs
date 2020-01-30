using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using UnityEngine;
using RWCustom;

public class RainConfig : OptionInterface
{
    public RainConfig() : base(RainScript.mod) //PartialityMod instance
    {
    }

    public OpLabel rainIntensity;
    public OpLabel rainIntensityDescription;
    public OpRadioButton intensityDynamic;
    public OpRadioButton intensityLow;
    public OpRadioButton intensityMed;
    public OpRadioButton intensityHigh;
    public OpLabel dynamicLabel;
    public OpLabel lowLabel;
    public OpLabel medLabel;
    public OpLabel highLabel;
    public OpRadioButtonGroup intensityGroup;
    public OpLabel lightningOption;
    public OpLabel lightningOptionDescription;
    public OpRadioButtonGroup lightningGroup;
    public OpRadioButton lightningOn;
    public OpRadioButton lightningOff;
    public OpLabel offLabel;
    public OpLabel onLabel;
    public OpLabel paletteOption;
    public OpLabel paletteOptionDescription;
    public OpRadioButtonGroup paletteGroup;
    public OpRadioButton paletteOn;
    public OpRadioButton paletteOff;
    public OpLabel offPaletteLabel;
    public OpLabel onPaletteLabel;

    public override void Initialize()
    {
        base.Initialize();
        this.Tabs = new OpTab[1];
        this.Tabs[0] = new OpTab("Options");
        //Rain
        rainIntensity = new OpLabel(new Vector2(30f, 550f), new Vector2(400f, 40f),"Rain Intensity",FLabelAlignment.Left, true);
        Tabs[0].AddItem(rainIntensity);
        rainIntensityDescription = new OpLabel(new Vector2(30f, 520f), new Vector2(400f, 40f), "Change the intensity of the rainfall to be dynamic, or a fixed value.", FLabelAlignment.Left, false);
        Tabs[0].AddItem(rainIntensityDescription);
        intensityGroup = new OpRadioButtonGroup("Setting", 0);
        lightningGroup = new OpRadioButtonGroup("Lightning", 1);
        paletteGroup = new OpRadioButtonGroup("Palette", 1);
        intensityDynamic = new OpRadioButton(new Vector2(30f, 490f));
        intensityDynamic.description = "Intensity of the rain is randomly determined and affected by karma level.";
        Tabs[0].AddItem(intensityDynamic);
        dynamicLabel = new OpLabel(new Vector2(60f, 485f), new Vector2(400f, 40f), "Dynamic", FLabelAlignment.Left, false);
        Tabs[0].AddItem(dynamicLabel);
        intensityLow = new OpRadioButton(new Vector2(30f, 460f));
        intensityLow.description = "Intensity of the rain will be fixed to Low intensity.";
        Tabs[0].AddItem(intensityLow);
        lowLabel = new OpLabel(new Vector2(60f, 455f), new Vector2(400f, 40f), "Low", FLabelAlignment.Left, false);
        Tabs[0].AddItem(lowLabel);
        intensityMed = new OpRadioButton(new Vector2(30f, 430f));
        intensityMed.description = "Intensity of the rain will be fixed to Medium intensity.";
        Tabs[0].AddItem(intensityMed);
        medLabel = new OpLabel(new Vector2(60f, 425f), new Vector2(400f, 40f), "Medium", FLabelAlignment.Left, false);
        Tabs[0].AddItem(medLabel);
        intensityHigh = new OpRadioButton(new Vector2(30f, 400f));
        intensityHigh.description = "Intensity of the rain will be fixed to High intensity.";
        Tabs[0].AddItem(intensityHigh);
        highLabel = new OpLabel(new Vector2(60f, 395f), new Vector2(400f, 40f), "High", FLabelAlignment.Left, false);
        Tabs[0].AddItem(highLabel);
        intensityGroup.SetButtons(new OpRadioButton[] { intensityDynamic, intensityLow, intensityMed, intensityHigh });
        Tabs[0].AddItem(intensityGroup);
        //Lightning
        lightningOption = new OpLabel(new Vector2(30f, 350f), new Vector2(400f, 40f), "Lightning", FLabelAlignment.Left, true);
        Tabs[0].AddItem(lightningOption);
        lightningOptionDescription = new OpLabel(new Vector2(30f, 320f), new Vector2(400f, 40f), "Determines whether a lightning effect is added during intense rain.", FLabelAlignment.Left, false);
        Tabs[0].AddItem(lightningOptionDescription);
        lightningOff = new OpRadioButton(new Vector2(30f, 300f));
        lightningOff.description = "Lightning effects will not be added during intense rain.";
        Tabs[0].AddItem(lightningOff);
        offLabel = new OpLabel(new Vector2(60f,295f), new Vector2(400f, 40f),"Off", FLabelAlignment.Left, false);
        Tabs[0].AddItem(offLabel);
        lightningOn = new OpRadioButton(new Vector2(30f, 270f));
        lightningOn.description = "Lightning effects will be added during intense rain.";
        Tabs[0].AddItem(lightningOn);
        onLabel = new OpLabel(new Vector2(60f, 265f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        Tabs[0].AddItem(onLabel);
        lightningGroup.SetButtons(new OpRadioButton[] { lightningOff, lightningOn});
        Tabs[0].AddItem(lightningGroup);
        //Palette
        paletteOption = new OpLabel(new Vector2(30f, 210f), new Vector2(400f, 40f), "Darkening", FLabelAlignment.Left, true);
        Tabs[0].AddItem(paletteOption);
        paletteOptionDescription = new OpLabel(new Vector2(30f, 180f), new Vector2(400f, 40f), "Change whether the rain intensity will darken the color palette.", FLabelAlignment.Left, false);
        Tabs[0].AddItem(paletteOptionDescription);
        paletteOff = new OpRadioButton(new Vector2(30f, 160f));
        paletteOff.description = "The region will not become darker with higher rain intensity.";
        Tabs[0].AddItem(paletteOff);
        offPaletteLabel = new OpLabel(new Vector2(60f, 155f), new Vector2(400f, 40f), "Off", FLabelAlignment.Left, false);
        Tabs[0].AddItem(offPaletteLabel);
        paletteOn = new OpRadioButton(new Vector2(30f, 130f));
        paletteOn.description = "The region will become darker with higher rain intensity";
        Tabs[0].AddItem(paletteOn);
        onPaletteLabel = new OpLabel(new Vector2(60f, 125f), new Vector2(400f, 40f), "On", FLabelAlignment.Left, false);
        Tabs[0].AddItem(onPaletteLabel);
        paletteGroup.SetButtons(new OpRadioButton[] { paletteOff, paletteOn });
        Tabs[0].AddItem(paletteGroup);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
    }
    public override void ConfigOnChange()
    {
        base.ConfigOnChange();
        if(config["Palette"] == "0")
        {
            RainPalette.paletteChange = false;
        }
        else
        {
            RainPalette.paletteChange = true;
        }
        if (config["Lightning"] == "0")
        {
            RainFall.lightning = false;
        }
        else
        {
            RainFall.lightning = true;
        }
        if (config["Setting"] == "0")
        {
            RainFall.intensity = 0;
        }
        if (config["Setting"] == "1")
        {
            RainFall.intensity = 1;
        }
        if (config["Setting"] == "2")
        {
            RainFall.intensity = 2;
        }
        if (config["Setting"] == "3")
        {
            RainFall.intensity = 3;
        }
    }
}