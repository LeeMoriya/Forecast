using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class WeatherForecast
{
    public static List<string> weatherlessRegions = new List<string>();
    public static Dictionary<string, int> regionWindDirection = new Dictionary<string, int>();
    public static Dictionary<string, float> dynamicRegionStartingIntensity = new Dictionary<string, float>();

    //Dictionary<string, dictionary<weatherType, float>>
    //In ForecastConfig, a dictionary containing save data of all region and the list of possible weathers that can happen in them.
    //Each weather has a percentage chance to occur -- unless the weather preference overrides it.
    //Weather preference will not kick in if the chosen weather is not one that's been enabled for that region
    //In that case, it will revert back to the percentage chances for the selected weathers
    //If it fails to generate a new weather type, it will do the same weather again
    public static Dictionary<string, Dictionary<Weather.WeatherType, float>> regionWeatherProbability = new Dictionary<string, Dictionary<Weather.WeatherType, float>>();
    public static Dictionary<string, List<Weather.WeatherType>> regionWeatherForecasts = new Dictionary<string, List<Weather.WeatherType>>();

    //Method to be replaced in ForecastConfig
    public static void GenerateWeathers()
    {
        if(regionWeatherProbability.Keys.Count > 0)
        {
            //Already generated weather probability
            return;
        }
        regionWeatherProbability = new Dictionary<string, Dictionary<Weather.WeatherType, float>>();
        foreach(string reg in ForecastConfig.regionSettings.Keys)
        {
            Dictionary<Weather.WeatherType, float> weathers = new Dictionary<Weather.WeatherType, float>();
            float chance = 1f / Enum.GetNames(typeof(Weather.WeatherType)).Length;

            weathers.Add(Weather.WeatherType.LightRain, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.HeavyRain, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.Thunderstorm, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.Fog, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.Hail, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.LightSnow, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.HeavySnow, UnityEngine.Random.value);
            weathers.Add(Weather.WeatherType.Blizzard, UnityEngine.Random.value);

            regionWeatherProbability.Add(reg, weathers);
        }
    }

    public static void InitialWeather()
    {
        if(regionWeatherForecasts.Keys.Count > 0)
        {
            //There are already existing forecasts
            ForecastLog.Log("Initial weather already generated");
            return;
        }
        regionWeatherForecasts = new Dictionary<string, List<Weather.WeatherType>>();
        foreach(string region in regionWeatherProbability.Keys)
        {
            ForecastLog.Log($"FORECAST FOR {region}");
            regionWeatherForecasts.Add(region, new List<Weather.WeatherType>());
            regionWeatherForecasts[region].Add(RandomWeather(region));
            regionWeatherForecasts[region].Add(NextWeather(region, regionWeatherForecasts[region][0]));
            regionWeatherForecasts[region].Add(NextWeather(region, regionWeatherForecasts[region][1]));
            ForecastLog.Log($"--------------------------------------------------------");
        }
        WeatherData.Save();
    }

    public static Weather.WeatherType NextWeather(string region, Weather.WeatherType lastWeather)
    {
        if (UnityEngine.Random.value < 0.75f)
        {
            float totalProbability = 0f;
            foreach (var pair in regionWeatherProbability[region])
            {
                totalProbability += pair.Value;
            }
            float rand = UnityEngine.Random.Range(0,totalProbability);

            List<Weather.WeatherType> shuffled = regionWeatherProbability[region].Keys.ToList();
            Shuffle(shuffled);

            for (int i = 0; i < shuffled.Count; i++)
            {
                rand -= regionWeatherProbability[region][shuffled[i]];
                if (rand <= 0f)
                {
                    ForecastLog.Log($"{region}: Preferred = {shuffled[i]}");
                    return shuffled[i];
                }
            }

            ForecastLog.Log($"{region}: Repeat = {lastWeather}");
            return lastWeather;
        }
        else
        {
            return RandomWeather(region);
        }
    }

    public static Weather.WeatherType RandomWeather(string region)
    {
        float rand = UnityEngine.Random.value;

        var nearestPair = regionWeatherProbability[region].OrderBy(kv => Math.Abs(kv.Value - rand)).First();
        ForecastLog.Log($"{region}: Random = {nearestPair.Key}");

        return nearestPair.Key;
    }

    static void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static void ClearAll()
    {
        weatherlessRegions.Clear();
        regionWindDirection.Clear();
        dynamicRegionStartingIntensity.Clear();
    }
    
    public class Weather
    {
        public WeatherType type;
        public float minIntensity;
        public float maxIntensity;
        public int weatherIndex;
        public Dictionary<WeatherType, float> nextPreference;

        public Weather(WeatherType type)
        {
            try
            {
                GenerateWeather(type);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void GenerateWeather(WeatherType type)
        {
            nextPreference = new Dictionary<WeatherType, float>();
            this.type = type;

            switch (type)
            {
                case WeatherType.LightRain:
                    weatherIndex = 0;
                    minIntensity = 0f + UnityEngine.Random.Range(0f, 0.2f);
                    maxIntensity = 0.65f;
                    nextPreference.Add(WeatherType.HeavyRain, 0.5f);
                    nextPreference.Add(WeatherType.Fog, 0.15f);
                    nextPreference.Add(WeatherType.Hail, 0.1f);
                    break;
                case WeatherType.HeavyRain:
                    weatherIndex = 0;
                    minIntensity = 0.5f + UnityEngine.Random.Range(0f, 0.2f);
                    maxIntensity = 1f;
                    nextPreference.Add(WeatherType.LightRain, 0.3f);
                    nextPreference.Add(WeatherType.Thunderstorm, 0.4f);
                    break;
                case WeatherType.Thunderstorm:
                    weatherIndex = 0;
                    minIntensity = 0.85f;
                    maxIntensity = 1f;
                    nextPreference.Add(WeatherType.LightRain, 0.45f);
                    nextPreference.Add(WeatherType.Fog, 0.4f);
                    nextPreference.Add(WeatherType.HeavyRain, 0.15f);
                    break;
                case WeatherType.Fog:
                    weatherIndex = 0;
                    minIntensity = 0f;
                    maxIntensity = 0.1f;
                    nextPreference.Add(WeatherType.LightRain, 0.35f);
                    nextPreference.Add(WeatherType.LightSnow, 0.2f);
                    break;
                case WeatherType.Hail:
                    weatherIndex = 0;
                    minIntensity = 0f;
                    maxIntensity = 0.75f;
                    nextPreference.Add(WeatherType.LightSnow, 0.35f);
                    nextPreference.Add(WeatherType.HeavyRain, 0.2f);
                    nextPreference.Add(WeatherType.Thunderstorm, 0.15f);
                    break;
                case WeatherType.LightSnow:
                    weatherIndex = 2;
                    minIntensity = 0f + UnityEngine.Random.Range(0f, 0.2f);
                    maxIntensity = 0.55f;
                    nextPreference.Add(WeatherType.HeavySnow, 0.6f);
                    nextPreference.Add(WeatherType.Blizzard, 0.3f);
                    nextPreference.Add(WeatherType.Hail, 0.1f);
                    break;
                case WeatherType.HeavySnow:
                    weatherIndex = 2;
                    minIntensity = 0.45f;
                    maxIntensity = 0.9f;
                    nextPreference.Add(WeatherType.LightSnow, 0.35f);
                    nextPreference.Add(WeatherType.Blizzard, 0.35f);
                    nextPreference.Add(WeatherType.Fog, 0.1f);
                    break;
                case WeatherType.Blizzard:
                    weatherIndex = 2;
                    minIntensity = 0.9f;
                    maxIntensity = 1f;
                    nextPreference.Add(WeatherType.LightRain, 0.1f);
                    nextPreference.Add(WeatherType.LightSnow, 0.3f);
                    nextPreference.Add(WeatherType.Hail, 0.1f);
                    nextPreference.Add(WeatherType.HeavySnow, 0.3f);
                    break;
            }
        }
        public enum WeatherType
        {
            LightRain,
            HeavyRain,
            Thunderstorm,
            Fog,
            Hail,
            LightSnow,
            HeavySnow,
            Blizzard,
        }
    }
}

