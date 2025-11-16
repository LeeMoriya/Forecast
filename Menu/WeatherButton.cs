using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
using UnityEngine;

namespace Menu
{
    public class WeatherButton : SymbolButton
    {
        public Color color = new HSLColor(1f, 0f, 0.8f).rgb;
        public bool enabled;
        public WeatherButton(Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos) : base(menu, owner, symbolName, singalText, pos)
        {
            enabled = true;
            color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
            num *= buttonBehav.sizeBump;
            symbolSprite.color = (buttonBehav.greyedOut || !enabled ? Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey) : Color.Lerp(this.color, Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), num));
            symbolSprite.x = DrawX(timeStacker) + base.DrawSize(timeStacker).x / 2f;
            symbolSprite.y = DrawY(timeStacker) + base.DrawSize(timeStacker).y / 2f;
            Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
            for (int i = 0; i < 9; i++)
            {
                roundedRect.sprites[i].color = color;
            }
        }
    }
}
