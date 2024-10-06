using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
using RWCustom;
using UnityEngine;
using Menu.Remix.MixedUI;
using Menu.Remix;
using System.IO;
using static WeatherForecast;

public class ForecastDialog : Dialog
{
    public float uAlpha;
    public float lastAlpha;
    public float currentAlpha;
    public float targetAlpha;
    public bool opening, closing;

    public float leftAnchor;
    public SimpleButton close;
    public FSprite leftShade, rightShade;

    public MenuTabWrapper tabWrapper;
    public OpScrollBox scrollBox;
    public RFPanelContainer panelContainer;
    public SimpleButton left, right, wipe;

    public ForecastDialog(ProcessManager manager) : base(manager)
    {
        WeatherData.Load();

        leftAnchor = Custom.GetScreenOffsets()[0];
        pages[0].pos.y += 2000f;
        pages[0].pos.x = leftAnchor + 0.01f;

        MenuLabel title = new MenuLabel(this, pages[0], "REGION FORECASTS", new Vector2(manager.rainWorld.options.ScreenSize.x / 2, 720f), new Vector2(), true);
        pages[0].subObjects.Add(title);

        MenuLabel desc = new MenuLabel(this, pages[0], "Adjust the weather probabilities for each region or disable it altogether", new Vector2(manager.rainWorld.options.ScreenSize.x / 2, 690f), new Vector2(), false);
        pages[0].subObjects.Add(desc);

        close = new SimpleButton(this, pages[0], "CLOSE", "CLOSE", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 -50f, 20f), new Vector2(100f, 30f));
        close.rectColor = new HSLColor(0f, 0.8f, 0.45f);
        close.labelColor = new HSLColor(0f, 0.8f, 0.45f);
        pages[0].subObjects.Add(close);

        panelContainer = new RFPanelContainer(this, pages[0], new Vector2(300f + leftAnchor, 150f));
        pages[0].subObjects.Add(panelContainer);

        left = new SimpleButton(this, pages[0], "<", "LEFT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 - 200f, 50f), new Vector2(30f, 30f));
        pages[0].subObjects.Add(left);

        right = new SimpleButton(this, pages[0], ">", "RIGHT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 + 200f - 30f, 50f), new Vector2(30f, 30f));
        pages[0].subObjects.Add(right);

        wipe = new SimpleButton(this, pages[0], "RESET FORECASTS", "wipe", new Vector2(manager.rainWorld.options.ScreenSize.x / 2 - 50f, 60f), new Vector2(100f, 30f));
        pages[0].subObjects.Add(wipe);

        leftShade = new FSprite("LinearGradient200", true);
        leftShade.x = 0f;
        leftShade.y = 0f;
        leftShade.rotation = 90f;
        leftShade.SetAnchor(1f,0f);
        leftShade.scaleX = 800f;
        leftShade.scaleY = 1.5f;
        leftShade.color = Color.black;
        container.AddChild(leftShade);

        rightShade = new FSprite("LinearGradient200", true);
        rightShade.x = manager.rainWorld.options.ScreenSize.x + 1f;
        rightShade.y = 0f;
        rightShade.rotation = 270f;
        rightShade.SetAnchor(0f, 0f);
        rightShade.scaleX = 800f;
        rightShade.scaleY = 1.5f;
        rightShade.color = Color.black;
        container.AddChild(rightShade);

        opening = true;
        targetAlpha = 1;
    }

    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message == "CLOSE")
        {
            PlaySound(SoundID.HUD_Exit_Game);
            targetAlpha = 0f;
            closing = true;
            WeatherData.Save();
        }
        if(message == "LEFT")
        {
            panelContainer.shift--;
            panelContainer.MoveToPoint(890f, 0.4f);
        }
        if (message == "RIGHT")
        {
            panelContainer.shift++;
            panelContainer.MoveToPoint(-890f, 0.4f);
        }
        if(message == "wipe")
        {
            WeatherForecast.WipeAllForecasts();
        }
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        if (opening || closing)
        {
            uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
            darkSprite.alpha = uAlpha * 0.92f;
        }
        pages[0].pos.y = Mathf.Lerp(manager.rainWorld.options.ScreenSize.y + 100f, 0.01f, uAlpha < 0.999f ? uAlpha : 1f);
        if (leftShade != null)
        {
            leftShade.alpha = uAlpha;
        }
        if (rightShade != null)
        {
            rightShade.alpha = uAlpha;
        }
    }

    public override void Update()
    {
        base.Update();

        lastAlpha = currentAlpha;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);

        if (opening && pages[0].pos.y <= 0.01f) { opening = false; }

        if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
        {
            manager.StopSideProcess(this);
            closing = false;
        }

        if(left != null)
        {
            left.buttonBehav.greyedOut = panelContainer.shift == 0 || panelContainer.animating;
        }
        if(right != null)
        {
            right.buttonBehav.greyedOut = panelContainer.shift == panelContainer.maxShift || panelContainer.animating;
        }
    }

    public class RFPanelContainer : PositionedMenuObject
    {
        public int shift = 0;
        public int maxShift = 0;
        public List<RegionForecastPanel> panels = new List<RegionForecastPanel>();
        public Vector2 startPos, targetPos;
        public bool animating;
        public float animTime, duration, speed;

        public RFPanelContainer(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            string[] array = new string[]
            {
                ""
            };
            string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
            if (File.Exists(path))
            {
                array = File.ReadAllLines(path);
            }

            List<string> regions = array.ToList();
            regions.Sort((l, r) => Region.GetRegionFullName(l, SlugcatStats.Name.White).CompareTo(Region.GetRegionFullName(r, SlugcatStats.Name.White)));
            maxShift = regions.Count / 4;

            int x = 0;
            int y = 0;
            int offsetCounter = 0;
            float groupOffset = 0f;
            for (int i = 0; i < regions.Count; i++)
            {
                if(i != 0 && i % 4 == 0)
                {
                    offsetCounter++;
                    groupOffset = 100f * offsetCounter;
                }

                RegionForecastPanel panel = new RegionForecastPanel(menu, this, new Vector2(10f + (395f * x) + groupOffset, 250f - (270f * y)), regions[i]);
                subObjects.Add(panel);
                panels.Add(panel);
                if (y == 1)
                {
                    y = 0;
                    x++;
                }
                else
                {
                    y++;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (animating)
            {
                animTime += 1f / 40f;
                float t = Mathf.Clamp01(animTime / duration);
                float ease = 1f - Mathf.Pow(1f - t, 2f);
                pos = Vector2.Lerp(startPos, targetPos, ease);
                pos = Vector2.Lerp(lastPos, pos, ease);

                if (animTime > duration)
                {
                    animating = false;
                    animTime = 0f;
                }
            }
        }

        public void MoveToPoint(float direction, float timeTaken)
        {
            animating = true;
            startPos = pos;
            targetPos = new Vector2(pos.x + direction, pos.y);
            lastPos = pos;
            duration = timeTaken;
        }
    }

    public class RegionForecastPanel : PositionedMenuObject, Slider.ISliderOwner
    {
        public Slider.SliderID weatherSlider = new Slider.SliderID("forecastWeather", false);

        public string acronym = "";
        public string regionName;
        public MenuLabel regionNameLabel;
        public List<FSprite> weatherSprites;
        public List<MenuLabel> weatherLabels;
        public List<SymbolButton> weatherButtons;
        public RoundedRect rect;

        public SimpleButton weatherStateButton;
        public SimpleButton editButton;
        public HorizontalSlider probabilitySlider;

        public Weather.WeatherType selectedWeather;

        public RegionForecastPanel(Menu.Menu menu, MenuObject owner, Vector2 pos, string region) : base(menu, owner, pos)
        {
            acronym = region;
            regionName = Region.GetRegionFullName(region, SlugcatStats.Name.White);
            rect = new RoundedRect(menu, this, new Vector2(), new Vector2(350f, 250f), true);
            regionNameLabel = new MenuLabel(menu, this, regionName, new Vector2(rect.size.x / 2, rect.size.y - 20f), new Vector2(), true);
            regionNameLabel.label.alignment = FLabelAlignment.Center;
            subObjects.Add(rect);
            subObjects.Add(regionNameLabel);

            int x = 0;
            weatherButtons = new List<SymbolButton>();
            weatherSprites = new List<FSprite>();
            weatherLabels = new List<MenuLabel>();

            if (regionWeatherProbability.ContainsKey(acronym))
            {
                selectedWeather = regionWeatherProbability[acronym].ElementAt(0).Key;
                foreach (KeyValuePair<Weather.WeatherType,float> weather in regionWeatherProbability[acronym])
                {
                    SymbolButton weatherButton = new SymbolButton(menu, this, $"{weather.Key}", $"{weather.Key}", new Vector2(20f + (45f * x), 145f));
                    weatherButton.size = new Vector2(35f, 35f);
                    weatherButton.roundedRect.size = weatherButton.size;
                    subObjects.Add(weatherButton);
                    weatherButtons.Add(weatherButton);

                    MenuLabel weatherLabel = new MenuLabel(menu, this, $"{Mathf.RoundToInt(weather.Value * 100)}%", new Vector2(40f + (45f * x), 130f), new Vector2(), false);
                    subObjects.Add(weatherLabel);
                    weatherLabels.Add(weatherLabel);
                    x++;
                }
            }

            probabilitySlider = new HorizontalSlider(menu, this, "", new Vector2(30f, 75f), new Vector2(270f, 0f), weatherSlider, false);
            subObjects.Add(probabilitySlider);

            weatherStateButton = new SimpleButton(menu, this, "FORECAST", "state", new Vector2(20f, 20f), new Vector2(80f, 30f));
            subObjects.Add(weatherStateButton);

            editButton = new SimpleButton(menu, this, "EDIT", "edit", new Vector2(rect.size.x - 100f, 20f), new Vector2(80f, 30f));
            subObjects.Add(editButton);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if(Enum.TryParse(message, out Weather.WeatherType weatherParse))
            {
                selectedWeather = weatherParse;
            }
        }

        public override void Update()
        {
            base.Update();
            if (regionWeatherProbability.ContainsKey(acronym) && weatherLabels != null && weatherButtons != null)
            {
                for (int i = 0; i < weatherLabels.Count; i++)
                {
                    weatherLabels[i].text = $"{Mathf.RoundToInt(regionWeatherProbability[acronym].ElementAt(i).Value * 100)}%";
                    weatherButtons[i].symbolSprite.color = regionWeatherProbability[acronym].ElementAt(i).Value > 0f ? new Color(1f, 1f, 1f) : new Color(0.4f, 0.4f, 0.4f);
                }
            }
        }

        public void SliderSetValue(Slider slider, float setValue)
        {
            regionWeatherProbability[acronym][selectedWeather] = setValue;
        }

        public float ValueOfSlider(Slider slider)
        {
            return regionWeatherProbability[acronym][selectedWeather];
        }
    }
}

