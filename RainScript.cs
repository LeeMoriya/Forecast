using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

//Change color to somewhat match palette, possibly with fog color
//Have rain fall even when not in the room
//Tie rain fall spawn radius to player position
//Have rain intensity be variable
//Implement palette darkening for more intense rain
//Rain intensity can be affected by Karma level, guarenteeing higher values at lower Karma

    //Config Options
    //Low, Med, High, Dynamic rain intensities
    //Enable or Disable palette darkening
    //Enable debug controls (Change intensity on the fly, enable or disable rain)

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

