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

        if(File.Exists(path))
        {
            string[] data = File.ReadAllLines(path);
            regionWeatherProbability = new Dictionary<string, Dictionary<Weather.WeatherType, float>>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].ToLower().StartsWith("weather"))
                    continue;
                if (data[i].ToLower().StartsWith("region"))
                    break;

                string reg = Regex.Split(data[i], "~")[0];
                string[] weathers = Regex.Split(Regex.Split(data[i], "~")[1], ">");

                Dictionary<Weather.WeatherType, float> regionWeathers = new Dictionary<Weather.WeatherType, float>();
                for (int s = 0; s < weathers.Length; s++)
                {
                    if(s == weathers.Length - 1)
                    {
                        continue;
                    }
                    string type = Regex.Split(weathers[s], "<")[0];
                    float chance = float.Parse(Regex.Split(weathers[s], "<")[1]);
                    Weather.WeatherType weatherType = (Weather.WeatherType)Enum.Parse(typeof(Weather.WeatherType), type);

                    regionWeathers.Add(weatherType, chance);
                }
                regionWeatherProbability.Add(reg, regionWeathers);
            }
        }
    }

    public static void Save()
    {
        string rootFolder = Application.persistentDataPath + Path.DirectorySeparatorChar;
        string path = rootFolder + "Forecast" + Path.DirectorySeparatorChar + "Forecast.txt";
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(rootFolder + "Forecast");
        }

        string data = "Weather Probabilities:\n";

        if(regionWeatherProbability != null && regionWeatherForecasts != null)
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
                    if(i < regionWeatherForecasts[reg].Count - 1)
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

