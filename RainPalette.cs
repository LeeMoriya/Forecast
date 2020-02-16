using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

class RainPalette
{
    public static void Patch()
    {
        On.RoomCamera.LoadPalette += RoomCamera_LoadPalette;
        On.RoomCamera.ApplyEffectColorsToPaletteTexture += RoomCamera_ApplyEffectColorsToPaletteTexture;
    }

    public static float darkness;
    public static Texture2D exterior1;
    public static Texture2D exterior2;
    public static Texture2D interior1;

    //Room effect color changes
    private static void RoomCamera_ApplyEffectColorsToPaletteTexture(On.RoomCamera.orig_ApplyEffectColorsToPaletteTexture orig, RoomCamera self, ref Texture2D texture, int color1, int color2)
    {
        if (Downpour.paletteChange && !RainFall.noRain)
        {
            if (RainFall.rainIntensity > 0f)
            {
                darkness = RainFall.rainIntensity * 0.3f;
            }
            else
            {
                darkness = 1f * 0.3f;
            }
            self.allEffectColorsTexture = new Texture2D(40, 4, TextureFormat.ARGB32, false);
            self.allEffectColorsTexture.anisoLevel = 0;
            self.allEffectColorsTexture.filterMode = FilterMode.Point;
            self.www = new WWW(string.Concat(new object[]
            {
            "file:///",
            Custom.RootFolderDirectory(),
            "Assets",
            Path.DirectorySeparatorChar,
            "Futile",
            Path.DirectorySeparatorChar,
            "Resources",
            Path.DirectorySeparatorChar,
            "Palettes",
            Path.DirectorySeparatorChar,
            "effectColors.png"
            }));
            self.www.LoadImageIntoTexture(self.allEffectColorsTexture);
            if (self.room != null)
            {
                if (color1 > -1)
                {
                    Color[] colors = self.allEffectColorsTexture.GetPixels();
                    Color[] newColors = new Color[colors.Length];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        if (!Downpour.snow)
                        {
                            newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                            newColors[i] = Color.Lerp(newColors[i], new Color(0f, 0f, 0f), darkness * 1.5f);
                            self.allEffectColorsTexture.SetPixels(newColors);
                        }
                        else
                        {
                            if (!RainFall.noRain)
                            {
                                newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                                newColors[i] = Color.Lerp(newColors[i], new Color(1f, 1f, 1f), darkness * 1.5f);
                                self.allEffectColorsTexture.SetPixels(newColors);
                            }
                            else
                            {
                                self.allEffectColorsTexture.SetPixels(colors);
                            }
                        }
                    }
                    texture.SetPixels(30, 4, 2, 2, self.allEffectColorsTexture.GetPixels(color1 * 2, 0, 2, 2, 0), 0);
                    texture.SetPixels(30, 12, 2, 2, self.allEffectColorsTexture.GetPixels(color1 * 2, 2, 2, 2, 0), 0);
                }
                if (color2 > -1)
                {
                    Color[] colors = self.allEffectColorsTexture.GetPixels();
                    Color[] newColors = new Color[colors.Length];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        if (!Downpour.snow)
                        {
                            newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                            newColors[i] = Color.Lerp(newColors[i], new Color(0f, 0f, 0f), darkness * 0.5f);
                            self.allEffectColorsTexture.SetPixels(newColors);
                        }
                        else
                        {
                            if (!RainFall.noRain)
                            {
                                newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                                self.allEffectColorsTexture.SetPixels(newColors);
                            }
                            else
                            {
                                self.allEffectColorsTexture.SetPixels(colors);
                            }
                        }
                    }
                    self.allEffectColorsTexture.SetPixels(newColors);
                    texture.SetPixels(30, 2, 2, 2, self.allEffectColorsTexture.GetPixels(color2 * 2, 0, 2, 2, 0), 0);
                    texture.SetPixels(30, 10, 2, 2, self.allEffectColorsTexture.GetPixels(color2 * 2, 2, 2, 2, 0), 0);
                }
            }
        }
        else
        {
            orig.Invoke(self, ref texture, color1, color2);
        }
    }

    //Main room palette changes
    public static void RoomCamera_LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref Texture2D texture)
    {
        if (Downpour.paletteChange && !RainFall.noRain)
        {
            if (RainFall.rainIntensity > 0f)
            {
                darkness = RainFall.rainIntensity * 0.3f;
            }
            else
            {
                darkness = 1f * 0.3f;
            }
            if (self.room == null)
            {
                //Fix for starting rooms
                self.room = self.loadingRoom;
            }
            texture = new Texture2D(32, 16, TextureFormat.ARGB32, false);
            texture.anisoLevel = 0;
            texture.filterMode = FilterMode.Point;
            exterior1 = new Texture2D(32, 16, TextureFormat.ARGB32, false);
            exterior1.anisoLevel = 0;
            exterior1.filterMode = FilterMode.Point;
            exterior2 = new Texture2D(32, 16, TextureFormat.ARGB32, false);
            exterior2.anisoLevel = 0;
            exterior2.filterMode = FilterMode.Point;
            interior1 = new Texture2D(32, 16, TextureFormat.ARGB32, false);
            interior1.anisoLevel = 0;
            interior1.filterMode = FilterMode.Point;
            {
                //Load regular palette texture
                self.www = new WWW(string.Concat(new object[]
                {
                "file:///",
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "Palettes",
                Path.DirectorySeparatorChar,
                "palette",
                pal,
                ".png"
                }));
                self.www.LoadImageIntoTexture(texture);
                //Load ext palette 1
                self.www = new WWW(string.Concat(new object[]
                {
                "file:///",
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "Palettes",
                Path.DirectorySeparatorChar,
                "snowExterior1.png"
                }));
                self.www.LoadImageIntoTexture(exterior1);
                //Load ext palette 2
                self.www = new WWW(string.Concat(new object[]
                {
                "file:///",
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "Palettes",
                Path.DirectorySeparatorChar,
                "snowExterior2.png"
                }));
                self.www.LoadImageIntoTexture(exterior2);
                //Load snow palette texture
                self.www = new WWW(string.Concat(new object[]
                {
                "file:///",
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "Palettes",
                Path.DirectorySeparatorChar,
                "snowInterior1.png"
                }));
                self.www.LoadImageIntoTexture(interior1);
            }
            if (self.room != null)
            {
                self.ApplyEffectColorsToPaletteTexture(ref texture, self.room.roomSettings.EffectColorA, self.room.roomSettings.EffectColorB);
            }
            //Colors from the palette texture are added to an array and desaturated and lerped towards a black color depending on rain intensity.
            if (self.room != null)
            {
                Color[] colors = texture.GetPixels();
                Color[] newColors = new Color[colors.Length];
                Color[] exterior1Cols = exterior1.GetPixels();
                Color[] exterior2Cols = exterior2.GetPixels();
                Color[] interior1Cols = interior1.GetPixels();
                for (int i = 0; i < colors.Length; i++)
                {
                    //Rain
                    if (!Downpour.snow)
                    {
                        newColors[i] = Custom.Desaturate(colors[i], darkness * 0.05f);
                        newColors[i] = Color.Lerp(newColors[i], new Color(0f, 0f, 0f), darkness * 0.05f);
                        texture.SetPixels(newColors);
                    }
                    //Snow
                    else
                    {
                        //Exterior
                        if (!RainFall.noRain && self.loadingRoom.roomSettings.DangerType == RoomRain.DangerType.Rain && self.loadingRoom.world.region.name != "HI")
                        {
                            newColors[i] = colors[i];
                            newColors[i] = Custom.Screen(newColors[i], exterior1Cols[i]);
                            texture.SetPixels(newColors);
                        }
                        else if (!RainFall.noRain && self.loadingRoom.roomSettings.DangerType == RoomRain.DangerType.Rain)
                        {
                            newColors[i] = colors[i];
                            newColors[i] = Custom.Screen(newColors[i], exterior2Cols[i]);
                            texture.SetPixels(colors);
                        }
                        else
                        {
                            newColors[i] = colors[i];
                            texture.SetPixels(colors);
                        }
                    }
                }
            }
            texture.Apply(true);
        }
        else
        {
            orig.Invoke(self, pal, ref texture);
        }
    }
}