using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using OptionalUI;

public class Downpour : PartialityMod
{
    public Downpour()
    {
        this.ModID = "Downpour";
        this.Version = "0000";
        this.author = "LeeMoriya";
    }

    public static RainScript script;

    public override void OnEnable()
    {
        base.OnEnable();
        RainScript.mod = this;
        GameObject go = new GameObject();
        script = go.AddComponent<RainScript>();
        script.Initialize();
    }
    public static OptionInterface LoadOI()
    {
        return new RainConfig();
    }
}