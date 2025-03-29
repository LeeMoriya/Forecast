using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using static WeatherForecast;

public static class WeatherData
{
    public static void Load()
    {
        string rootFolder = Application.persistentDataPath + Path.DirectorySeparatorChar;
        string path = rootFolder + "Forecast" + Path.DirectorySeparatorChar + "Forecast.txt";
        List<string> regions = new List<string>();
        bool forecastSection = false;

        string[] array = new string[]
        {
            ""
        };
        string regionPath = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
        if (File.Exists(path))
        {
            array = File.ReadAllLines(regionPath);
        }
        for (int i = 0; i < array.Length; i++)
        {
            if (!regions.Contains(array[i]))
            {
                regions.Add(array[i]);
                ForecastLog.Log($"Loading region... {array[i]}");
            }
        }

        //Assign default probabilities if none present
        regionWeatherProbability = new Dictionary<string, Dictionary<Weather.WeatherType, float>>();

        Dictionary<Weather.WeatherType, float> defaultWeathers = new Dictionary<Weather.WeatherType, float>
        {
            { Weather.WeatherType.LightRain, 0.3f },
            { Weather.WeatherType.HeavyRain, 0.3f },
            { Weather.WeatherType.Thunderstorm, 0.15f },
            { Weather.WeatherType.Fog, 0.05f },
            { Weather.WeatherType.LightSnow, 0.1f },
            { Weather.WeatherType.HeavySnow, 0.05f },
            { Weather.WeatherType.Blizzard, 0.05f }
        };

        regionWeatherProbability.Add("GLOBAL", defaultWeathers);
        foreach (string reg in regions)
        {
            // Add a new entry for the region with a dictionary of weather types and probabilities
            regionWeatherProbability.Add(reg, defaultWeathers);
            ForecastLog.Log($"Adding {reg} to regionWeatherProbability");
        }

        if (File.Exists(path))
        {
            string[] data = File.ReadAllLines(path);
            regionWeatherForecasts = new Dictionary<string, List<Weather.WeatherType>>();
            //regionWeatherProbability = new Dictionary<string, Dictionary<Weather.WeatherType, float>>();

            for (int i = 0; i < data.Length; i++)
            {
                if (!forecastSection)
                {
                    if (data[i].ToLower().StartsWith("weather"))
                        continue;
                    if (data[i].ToLower().StartsWith("region"))
                    {
                        forecastSection = true;
                        continue;
                    }

                    string reg = Regex.Split(data[i], "~")[0];
                    string[] weathers = Regex.Split(Regex.Split(data[i], "~")[1], ">");

                    Dictionary<Weather.WeatherType, float> regionWeathers = new Dictionary<Weather.WeatherType, float>();
                    for (int s = 0; s < weathers.Length; s++)
                    {
                        if (s == weathers.Length - 1)
                        {
                            continue;
                        }
                        string type = Regex.Split(weathers[s], "<")[0];
                        float chance = float.Parse(Regex.Split(weathers[s], "<")[1]);
                        Weather.WeatherType weatherType = (Weather.WeatherType)Enum.Parse(typeof(Weather.WeatherType), type);

                        regionWeathers.Add(weatherType, chance);
                    }
                    if (!regionWeatherProbability.ContainsKey(reg))
                    {
                        regionWeatherProbability.Add(reg, regionWeathers);
                    }
                    else
                    {
                        regionWeatherProbability[reg] = regionWeathers;
                    }
                }
                else
                {
                    string reg = Regex.Split(data[i], "~")[0];
                    string[] weathers = Regex.Split(Regex.Split(data[i], "~")[1], ":");

                    List<Weather.WeatherType> forecast = new List<Weather.WeatherType>();
                    for (int s = 0; s < weathers.Length; s++)
                    {
                        Weather.WeatherType weatherType = (Weather.WeatherType)Enum.Parse(typeof(Weather.WeatherType), weathers[s]);
                        forecast.Add(weatherType);
                    }

                    if (!regionWeatherForecasts.ContainsKey(reg))
                    {
                        regionWeatherForecasts.Add(reg, forecast);
                        ForecastLog.Log($"LOAD: {reg} forecast is {forecast[0]} | {forecast[1]} | {forecast[2]}");
                    }
                }
            }
        }
        else
        {
            //If there is no save file for weather probabilities, load defaults from a built-in text file
        }
    }

    public static void Save() //TODO - this doesn't account for slugcat or save slot
    {
        string rootFolder = Application.persistentDataPath + Path.DirectorySeparatorChar;
        string path = rootFolder + "Forecast" + Path.DirectorySeparatorChar + "Forecast.txt";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(rootFolder + "Forecast");
        }

        string data = "Weather Probabilities:\n";

        if (regionWeatherProbability != null && regionWeatherForecasts != null)
        {
            foreach (string reg in regionWeatherProbability.Keys)
            {
                //Region Name
                data += $"{reg}~";
                //Weather types and probabilities
                foreach (Weather.WeatherType type in regionWeatherProbability[reg].Keys)
                {
                    data += $"{type}<{regionWeatherProbability[reg][type]}>";
                }
                data += "\n";
            }
            data += "Region Forecasts:\n";

            foreach (string reg in regionWeatherForecasts.Keys)
            {
                //Region Name
                data += $"{reg}~";
                for (int i = 0; i < regionWeatherForecasts[reg].Count; i++)
                {
                    data += $"{regionWeatherForecasts[reg][i]}";
                    if (i < regionWeatherForecasts[reg].Count - 1)
                    {
                        data += ":";
                    }
                }
                data += "\n";
            }
            File.WriteAllText(path, data);
        }
    }
}

