using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

using GlobalEnums;


namespace Nightmare_Spark
{
    public class SaveSettings
    {
        // insert default values here
        public bool[] gotCharms = new[] { false };
        public bool[] newCharms = new[] { false };
        public bool[] equippedCharms = new[] { false };
        public int[] charmCosts = new[] { 2 };
        public static bool dP = false;
        public bool PlacedCharm = false;
    }

    public class TextureStrings
    {
        #region Misc
        public const string NightmareSparkKey = "NightmareSpark";
        private const string NightmareSparkFile = "Nightmare_Spark.Resources.NightmareSpark.png";
       
        #endregion Misc

        private readonly Dictionary<string, Sprite> _dict;

        public TextureStrings()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            _dict = new Dictionary<string, Sprite>();
            Dictionary<string, string> tmpTextures = new Dictionary<string, string>();
            tmpTextures.Add(NightmareSparkKey, NightmareSparkFile);
            foreach (var t in tmpTextures)
            {
                using (Stream s = asm.GetManifestResourceStream(t.Value))
                {
                    if (s == null) continue;

                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();

                    //Create texture from bytes
                    var tex = new Texture2D(2, 2);

                    tex.LoadImage(buffer, true);

                    // Create sprite from texture
                    // Split is to cut off the TestOfTeamwork.Resources. and the .png
                    _dict.Add(t.Key, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                }
            }
        }

        public Sprite Get(string key)
        {
            return _dict[key];
        }
    }
}

