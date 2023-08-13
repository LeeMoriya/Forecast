using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WeatherForecast
{
    //The WeatherForecast object is created at the start of each cycle, alongside the OverWorld.
    //In a future update, its purpose will be to determine the current weather based on the ever changing
    //level of precipitation and temperature. By checking this it will decide whether regions should feature
    //rain, hail or snow this cycle and the intensity of each. It will also show the forecast for the next
    //few cycles.

    //Currently, the WeatherForecast's function will be to determine whether regions will have weather
    //this cycle based on the global weatherChance variable defined in REMIX or a custom settings file.

    public List<string> weatherlessRegions;
    public Dictionary<string, float> dynamicRegionStartingIntensity;
    public Dictionary<string, int> regionWindDirection;

    public WeatherForecast() 
    {
        weatherlessRegions = new List<string>();
        dynamicRegionStartingIntensity = new Dictionary<string, float>();
        regionWindDirection= new Dictionary<string, int>();
    }

    public void PrepareCycle()
    {
        weatherlessRegions = new List<string>();
        dynamicRegionStartingIntensity = new Dictionary<string, float>();
        regionWindDirection = new Dictionary<string, int>();
    }
}

