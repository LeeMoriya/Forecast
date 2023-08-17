using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static WeatherForecast;


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
    public List<Weather> availableWeather;

    public float lastLastTemperature = -1f;
    public float lastLastPrecipitation = -1f;
    public float lastTemperature = -1f;
    public float lastPrecipitation = -1f;
    public float currentTemperature = -1f;
    public float currentPrecipitation = -1f;

    public float averageTemperature;
    public float averagePrecipitation;

    public int lastForecast;
    public int thisForecast;
    public int nextForecast;

    public System.Random random;

    public WeatherForecast()
    {
        weatherlessRegions = new List<string>();
        dynamicRegionStartingIntensity = new Dictionary<string, float>();
        regionWindDirection = new Dictionary<string, int>();
        GenerateAvailableWeather();
        random = new System.Random();

        //Load temp and prec settings, generate new ones between the min and max if none exist
        PrepareCycle();
    }

    public void PrepareCycle()
    {
        random = new System.Random();
        weatherlessRegions = new List<string>();
        dynamicRegionStartingIntensity = new Dictionary<string, float>();
        regionWindDirection = new Dictionary<string, int>();

        bool initialWeather = false;
        //Generate initial precipitation
        if (currentPrecipitation == -1f)
        {
            initialWeather = true;
            lastLastPrecipitation = UnityEngine.Random.value;
            lastPrecipitation = UnityEngine.Random.value;
            currentPrecipitation = PredictPrecipitationChange(lastLastPrecipitation, lastPrecipitation);
        }
        //Generate initial temperature
        if (currentTemperature == -1f)
        {
            initialWeather = true;
            lastLastTemperature = UnityEngine.Random.value;
            lastTemperature = UnityEngine.Random.value;
            currentTemperature = PredictPrecipitationChange(lastLastTemperature, lastTemperature);
        }
        if(initialWeather)
        {
            lastForecast = GenerateForecast(lastLastTemperature, lastLastPrecipitation);
            thisForecast = GenerateForecast(lastTemperature, lastPrecipitation);
            nextForecast = GenerateForecast(currentTemperature, currentPrecipitation);
            ForecastLog.Log($"Weather Forecast: {lastForecast},{thisForecast},{nextForecast}");
            ForecastLog.Log($"Temperature: {lastLastTemperature},{lastTemperature},{currentTemperature}");
            ForecastLog.Log($"Precipitation: {lastLastPrecipitation},{lastPrecipitation},{currentPrecipitation}");
        }
        else
        {
            lastLastTemperature = lastTemperature;
            lastTemperature = currentTemperature;
            currentTemperature = PredictTemperatureChange(lastLastTemperature, lastTemperature);
            lastLastPrecipitation = lastPrecipitation;
            lastPrecipitation = currentPrecipitation;
            currentPrecipitation = PredictPrecipitationChange(lastLastPrecipitation, lastPrecipitation);

            lastForecast = thisForecast;
            thisForecast = nextForecast;
            nextForecast = GenerateForecast(currentTemperature, currentPrecipitation);

            ForecastLog.Log($"Prev: {availableWeather.Find(x => x.index == lastForecast).name}");
            ForecastLog.Log($"Now: {availableWeather.Find(x => x.index == thisForecast).name}");
            ForecastLog.Log($"Next: {availableWeather.Find(x => x.index == nextForecast).name}");
        }
    }

    public int GenerateForecast(float temp, float prec)
    {
        List<Weather> candidates = new List<Weather>();
        candidates.AddRange(availableWeather.FindAll(x => x.minTemperature <= temp && x.maxTemperature >= temp));
        if(candidates.Count > 0)
        {
            Weather w = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            ForecastLog.Log($"Weather Candidate: {w.name} - T: {currentTemperature:n2} - P: {currentPrecipitation:n2}");
            return w.index;
        }
        else
        {
            candidates.Add(availableWeather[UnityEngine.Random.Range(0, availableWeather.Count)]);
            if (candidates.Count > 0)
            {
                foreach (Weather weather in candidates)
                {
                    ForecastLog.Log($"Random Candidate: {weather.name} - T: {currentTemperature:n2} - P: {currentPrecipitation:n2}");
                }
                return candidates[0].index;
            }
            ForecastLog.Log("FAILED");
            return 0;
        }
    }

    public float PredictTemperatureChange(float lastLastTemperature, float lastTemperature)
    {
        float change = lastTemperature - lastLastTemperature;
        bool isIncreasing = change > 0;

        // Apply chance of opposite change
        if (UnityEngine.Random.value < 0.2f)
            isIncreasing = !isIncreasing;

        if (lastTemperature >= 0.9f)
        {
            isIncreasing = false;
        }
        if (lastTemperature <= 0.1f)
        {
            isIncreasing = true;
        }

        // Calculate predicted change
        float predictedChange = UnityEngine.Random.Range(0.1f, 0.25f);
        if (!isIncreasing)
            predictedChange *= -1;

        // Calculate predicted temperature
        float predictedTemperature = lastTemperature + predictedChange;

        if (predictedTemperature < averageTemperature)
        {
            predictedTemperature += 0.1f;
        }
        else if (predictedTemperature > averageTemperature)
        {
            predictedTemperature -= 0.1f;
        }
        return Mathf.Clamp(predictedTemperature, 0f, 1f);
    }

    public float PredictPrecipitationChange(float lastLastPrecipitation, float lastPrecipitation)
    {
        float change = lastPrecipitation - lastLastPrecipitation;
        bool isIncreasing = change > 0;

        // Apply chance of opposite change
        if (UnityEngine.Random.value < 0.2f)
            isIncreasing = !isIncreasing;

        if (lastPrecipitation >= 1f)
        {
            isIncreasing = false;
        }
        if (lastPrecipitation <= 0f)
        {
            isIncreasing = true;
        }

        // Calculate predicted change
        float predictedChange = UnityEngine.Random.Range(0.1f, 0.3f);
        if (!isIncreasing)
            predictedChange *= -1;

        // Calculate predicted precipitation
        float predictedPrecipitation = lastPrecipitation + predictedChange;

        if(predictedPrecipitation < averagePrecipitation)
        {
            predictedPrecipitation += 0.1f;
        }
        else if(predictedPrecipitation > averagePrecipitation)
        {
            predictedPrecipitation -= 0.1f;
        }
        return Mathf.Clamp(predictedPrecipitation, 0f, 1f);
    }

    public void GenerateAvailableWeather()
    {
        availableWeather = new List<Weather>
        {
            new Weather
            {
                index = 0,
                name = "Clear Skies",
                weatherType = -1,
                minPrecipitation = 0f,
                maxPrecipitation = 0.2f,
                minTemperature = 0.5f,
                maxTemperature = 0.85f
            },
            new Weather
            {
                index = 1,
                name = "Light Rain",
                weatherType = 0,
                minPrecipitation = 0.1f,
                maxPrecipitation = 0.5f,
                minTemperature = 0.1f,
                maxTemperature = 0.5f
            },
            new Weather
            {
                index = 2,
                name = "Heavy Rain",
                weatherType = 0,
                minPrecipitation = 0.5f,
                maxPrecipitation = 1f,
                minTemperature = 0.5f,
                maxTemperature = 0.85f
            },
            new Weather
            {
                index = 3,
                name = "Thunderstorm",
                weatherType = 0,
                minPrecipitation = 0.8f,
                maxPrecipitation = 1f,
                minTemperature = 0.85f,
                maxTemperature = 0.1f
            },
            new Weather
            {
                index = 4,
                name = "Light Hail",
                weatherType = 0,
                minPrecipitation = 0.2f,
                maxPrecipitation = 0.4f,
                minTemperature = 0.85f,
                maxTemperature = 1f
            },
            new Weather
            {
                index = 5,
                name = "Heavy Hail",
                weatherType = 0,
                minPrecipitation = 0.4f,
                maxPrecipitation = 0.65f,
                minTemperature = 0.85f,
                maxTemperature = 1f
            },
            new Weather
            {
                index = 6,
                name = "Light Snow",
                weatherType = 1,
                minPrecipitation = 0.1f,
                maxPrecipitation = 0.5f,
                minTemperature = 0.3f,
                maxTemperature = 0.5f
            },
            new Weather
            {
                index = 7,
                name = "Heavy Snow",
                weatherType = 1,
                minPrecipitation = 0.5f,
                maxPrecipitation = 1f,
                minTemperature = 0.15f,
                maxTemperature = 0.3f
            },
            new Weather
            {
                index = 8,
                name = "Blizzard",
                weatherType = 1,
                minPrecipitation = 0.8f,
                maxPrecipitation = 1f,
                minTemperature = 0f,
                maxTemperature = 0.15f
            }
        };
    }

    public class Weather
    {
        public int index;
        public string name; //Name for this weather in menus
        public string spriteName; //Sprite name used for the forecast

        public int weatherType; //Matches index for weather type

        public float minTemperature; //Temperature must be above this value to occur
        public float maxTemperature; //Temperature must be below this value to occur

        public float minPrecipitation; //Precipitation must be above this value to occur
        public float maxPrecipitation; //Precipitation must be below this value to occur
    }
}

