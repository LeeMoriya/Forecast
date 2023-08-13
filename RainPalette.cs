using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

class RainPalette
{
    public static float darkness;
    public static void Patch()
    {
        On.RoomCamera.LoadPalette += LoadPalette;
        On.RoomCamera.ApplyPalette += RoomCamera_ApplyPalette;
        On.RoomCamera.ApplyEffectColorsToPaletteTexture += RoomCamera_ApplyEffectColorsToPaletteTexture;
    }

    private static void RoomCamera_ApplyEffectColorsToPaletteTexture(On.RoomCamera.orig_ApplyEffectColorsToPaletteTexture orig, RoomCamera self, ref Texture2D texture, int color1, int color2)
    {
        orig.Invoke(self, ref texture, color1, color2);
        //if (ForecastMod.snow && ForecastMod.effectColors)
        //{
        //    if (color1 > -1)
        //    {
        //        Color[] effects1A = self.allEffectColorsTexture.GetPixels(color1 * 2, 0, 2, 2, 0);
        //        Color[] effects1B = self.allEffectColorsTexture.GetPixels(color1 * 2, 2, 2, 2, 0);
        //        Color[] snow1A = new Color[effects1A.Length];
        //        Color[] snow1B = new Color[effects1B.Length];
        //        for (int i = 0; i < snow1A.Length; i++)
        //        {
        //            snow1A[i] = Custom.Desaturate(effects1A[i], 0.1f);
        //            snow1A[i] = Color.Lerp(snow1A[i], new Color(1f, 1f, 1f), 0.6f);
        //        }
        //        for (int i = 0; i < snow1B.Length; i++)
        //        {
        //            snow1B[i] = Custom.Desaturate(effects1B[i], 0.1f);
        //            snow1B[i] = Color.Lerp(snow1B[i], new Color(1f, 1f, 1f), 0.6f);
        //        }
        //        texture.SetPixels(30, 4, 2, 2, snow1A, 0);
        //        texture.SetPixels(30, 12, 2, 2, snow1B, 0);
        //    }
        //    if (color2 > -1)
        //    {
        //        Color[] effects2A = self.allEffectColorsTexture.GetPixels(color2 * 2, 0, 2, 2, 0);
        //        Color[] effects2B = self.allEffectColorsTexture.GetPixels(color2 * 2, 2, 2, 2, 0);
        //        Color[] snow2A = new Color[effects2A.Length];
        //        Color[] snow2B = new Color[effects2B.Length];
        //        for (int i = 0; i < snow2A.Length; i++)
        //        {
        //            snow2A[i] = Custom.Desaturate(effects2A[i], 0.1f);
        //            snow2A[i] = Color.Lerp(snow2A[i], new Color(1f, 1f, 1f), 0.6f);
        //        }
        //        for (int i = 0; i < snow2B.Length; i++)
        //        {
        //            snow2B[i] = Custom.Desaturate(effects2B[i], 0.1f);
        //            snow2B[i] = Color.Lerp(snow2B[i], new Color(1f, 1f, 1f), 0.6f);
        //        }
        //        texture.SetPixels(30, 2, 2, 2, snow2A, 0);
        //        texture.SetPixels(30, 10, 2, 2, snow2B, 0);
        //    }
        //    if (ForecastConfig.debugMode.Value)
        //    {
        //        byte[] img = texture.EncodeToPNG();
        //        File.WriteAllBytes(Custom.RootFolderDirectory() + "snowPalette.png", img);
        //    }
        //}
    }

    private static void RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self)
    {
        orig.Invoke(self);
        if (ForecastConfig.weatherType.Value == 1)
        {
            self.currentPalette.shortCutSymbol = new Color(1f, 0.7f, 0f);
        }
    }

    private static void LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref Texture2D texture)
    {
        orig.Invoke(self, pal, ref texture);
        if (ForecastMod.paletteChange)
        {
            Room room = null;
            if (self.loadingRoom != null)
            {
                room = self.loadingRoom;
            }
            else if (self.room != null)
            {
                room = self.room;
            }
            if (room != null)
            {
                //Modify paletteTexture before orig is called.
                Color[] origPalette = texture.GetPixels();
                Color[] modifiedPalette = new Color[origPalette.Length];
                float darknessFade = 1f - Mathf.Lerp(0f, texture.GetPixel(30, 7).r, room.roomSettings.RainIntensity);
                if (self.game.IsArenaSession)
                {
                    Color[] colors = texture.GetPixels();
                    Color[] newColors = new Color[colors.Length];
                    Color[] exterior1Cols = ForecastMod.snowExt1.GetPixels();
                    Color[] interior1Cols = ForecastMod.snowInt1.GetPixels();
                    for (int i = 0; i < colors.Length; i++)
                    {
                        //Rain
                        if (ForecastConfig.weatherType.Value == 0)
                        {
                            newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                            newColors[i] = Color.Lerp(newColors[i], new Color(0f, 0f, 0f), darkness * 0.05f);
                            texture.SetPixels(newColors);
                        }
                        //Snow
                        else
                        {
                            if (WeatherHooks.roomSettings.TryGetValue(self.loadingRoom, out WeatherController.WeatherSettings s))
                            {
                                newColors[i] = colors[i];
                                exterior1Cols[i] = Color.Lerp(exterior1Cols[i], new Color(0f, 0f, 0f), 0f);
                                newColors[i] = Custom.Screen(newColors[i], exterior1Cols[i]);
                                texture.SetPixels(newColors);
                            }
                            else
                            {
                                newColors[i] = colors[i];
                                newColors[i] = Custom.Screen(newColors[i], interior1Cols[i]);
                                texture.SetPixels(newColors);
                            }
                        }
                    }
                }
                else
                {
                    if (ForecastConfig.weatherType.Value == 1)
                    {
                        //Determine palette needed
                        if (ForecastMod.rainRegions.Contains(room.world.region.name))
                        {
                            //Exterior
                            if (WeatherHooks.roomSettings.TryGetValue(self.loadingRoom, out WeatherController.WeatherSettings s))
                            {
                                Color[] snowPalette = ForecastMod.snowExt1.GetPixels();
                                for (int i = 0; i < modifiedPalette.Length; i++)
                                {
                                    modifiedPalette[i] = origPalette[i];
                                    modifiedPalette[i] = Custom.Desaturate(modifiedPalette[i], Mathf.Lerp(0.45f, 0f, darknessFade));
                                    snowPalette[i] = Color.Lerp(snowPalette[i], new Color(0f, 0f, 0f), darknessFade);
                                    modifiedPalette[i] = Custom.Screen(modifiedPalette[i], snowPalette[i]);
                                }
                            }
                            //Interior
                            else
                            {
                                Color[] snowPalette = ForecastMod.snowInt1.GetPixels();
                                for (int i = 0; i < modifiedPalette.Length; i++)
                                {
                                    modifiedPalette[i] = origPalette[i];
                                    modifiedPalette[i] = Custom.Desaturate(modifiedPalette[i], Mathf.Lerp(0.45f, 0f, darknessFade));
                                    snowPalette[i] = Color.Lerp(snowPalette[i], new Color(0f, 0f, 0f), darknessFade);
                                    modifiedPalette[i] = Custom.Screen(modifiedPalette[i], snowPalette[i]);
                                }
                            }
                            texture.SetPixels(modifiedPalette);
                            //Shortcut Symbol
                        }
                    }
                    else
                    {
                        Color[] colors = texture.GetPixels();
                        Color[] newColors = new Color[colors.Length];
                        for (int i = 0; i < colors.Length; i++)
                        {
                            newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                            newColors[i] = Color.Lerp(newColors[i], new Color(0f, 0f, 0f), darkness * 0.05f);
                        }
                        texture.SetPixels(newColors);
                    }
                }
            }
        }
    }
}