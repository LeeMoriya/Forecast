using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ForecastDialog2 : Dialog
{
    public string region;

    public float uAlpha;
    public float lastAlpha;
    public float currentAlpha;
    public float targetAlpha;
    public bool opening, closing;

    public float leftAnchor;
    public SimpleButton close, save;
    public FSprite leftShade, rightShade;

    public MenuTabWrapper tabWrapper;
    public OpScrollBox scrollBox;
    public SimpleButton left, right, wipe;

    public ForecastDialog2(ProcessManager manager, string region) : base(manager)
    {
        WeatherData.Load();
        this.region = region;

        leftAnchor = Custom.GetScreenOffsets()[0];
        pages[0].pos.y += 2000f;
        pages[0].pos.x = leftAnchor + 0.01f;

        leftShade = new FSprite("LinearGradient200", true);
        leftShade.x = 0f;
        leftShade.y = 0f;
        leftShade.rotation = 90f;
        leftShade.SetAnchor(1f, 0f);
        leftShade.scaleX = 800f;
        leftShade.scaleY = 6.5f;
        leftShade.color = Color.black;
        container.AddChild(leftShade);

        rightShade = new FSprite("LinearGradient200", true);
        rightShade.x = manager.rainWorld.options.ScreenSize.x + 1f;
        rightShade.y = 0f;
        rightShade.rotation = 270f;
        rightShade.SetAnchor(0f, 0f);
        rightShade.scaleX = 800f;
        rightShade.scaleY = 2.5f;
        rightShade.color = Color.black;
        container.AddChild(rightShade);

        MenuLabel title = new MenuLabel(this, pages[0], "EDIT FORECAST", new Vector2(850, 630f), new Vector2(), true);
        pages[0].subObjects.Add(title);

        MenuLabel desc = new MenuLabel(this, pages[0], "Adjust the probability that each weather will occur, or disable certain weathers entirely.", new Vector2(850, 600f), new Vector2(), false);
        pages[0].subObjects.Add(desc);

        ForecastDialog.RegionForecastPanel panel = new ForecastDialog.RegionForecastPanel(this, pages[0], new Vector2(670f, 300f), region);
        pages[0].subObjects.Add(panel);

        MenuLabel warning = new MenuLabel(this, pages[0], "WARNING: Saving will reset any existing region weathers.", new Vector2(850, 260f), new Vector2(), false);
        pages[0].subObjects.Add(warning);

        close = new SimpleButton(this, pages[0], "CLOSE", "CLOSE", new Vector2(740, 200f), new Vector2(100f, 30f));
        close.rectColor = new HSLColor(0f, 0.8f, 0.45f);
        close.labelColor = new HSLColor(0f, 0.8f, 0.45f);
        pages[0].subObjects.Add(close);

        save = new SimpleButton(this, pages[0], "SAVE", "SAVE", new Vector2(860, 200f), new Vector2(100f, 30f));
        save.rectColor = new HSLColor(0.39f, 0.8f, 0.45f);
        save.labelColor = new HSLColor(0.39f, 0.8f, 0.45f);
        pages[0].subObjects.Add(save);

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
        }
        if (message == "SAVE")
        {
            PlaySound(SoundID.HUD_Exit_Game);
            targetAlpha = 0f;
            closing = true;
            ForecastConfig.preferenceUpdate = true;
            WeatherForecast.regionWeatherForecasts.Clear();
            WeatherData.Save();
        }
        if (message == "wipe")
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
            darkSprite.alpha = uAlpha * 0.82f;
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
    }
}