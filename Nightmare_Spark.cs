global using System;
global using System.Collections;
global using System.Collections.Generic;
global using Modding;
global using UnityEngine;
global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;
global using Satchel;
global using Satchel.Futils;
global using GlobalEnums;
global using SFCore;
global using SFCore.Generics;
global using ItemChanger;
global using ItemChanger.Tags;
global using ItemChanger.UIDefs;
global using ItemChanger.Locations;
global using MonoMod.RuntimeDetour;
global using System.Reflection;


namespace Nightmare_Spark
{

    public class Nightmare_Spark : SaveSettingsMod<SaveSettings>
    {
        public static Nightmare_Spark Instance;

        new public string GetName() => "NightmareSpark";
        public override string GetVersion() => "V1.0";
        public Nightmare_Spark() : base("Nightmare Spark")
        {
            Ts = new TextureStrings();

        }
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>()
            {
                ("GG_Grimm_Nightmare", "Grimm Control/Nightmare Grimm Boss"),
                ("GG_Grimm_Nightmare", "Grimm Control/Grimm Bats/Real Bat"),
                ("GG_Grimm_Nightmare", "Grimm Spike Holder/Nightmare Spike"),
                ("GG_Grimm", "Grimm Spike Holder/Grimm Spike"),
                ("Abyss_02", "Flamebearer Spawn")
            };
        }
        public static TextureStrings Ts { get; private set; }
        public List<int> CharmIDs { get; private set; }

        public static GameObject? myTrail;
        public static GameObject? nkg;
        public static GameObject? burst;
        public static GameObject? realBat;
        public static GameObject? grimmkinSpawner;
        public static GameObject? nightmareSpike;
        public static GameObject? grimmSpike;
        public static AudioSource? audioSource;
        public static tk2dSpriteAnimation? spikeAnimation;
        public bool Placed = false;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            nkg = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Control/Nightmare Grimm Boss"];
            realBat = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Control/Grimm Bats/Real Bat"];
            nightmareSpike = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Spike Holder/Nightmare Spike"];
            grimmSpike = preloadedObjects["GG_Grimm"]["Grimm Spike Holder/Grimm Spike"];
            grimmkinSpawner = preloadedObjects["Abyss_02"]["Flamebearer Spawn"];
            GameObject.DontDestroyOnLoad(nkg);
            GameObject.DontDestroyOnLoad(realBat);
            Instance ??= this;
            var go = new GameObject("AudioSource");
            audioSource = go.AddComponent<AudioSource>();
            audioSource.pitch = .75f;
            audioSource.volume = .01f;
            UnityEngine.Object.DontDestroyOnLoad(audioSource);

            CharmIDs = CharmHelper.AddSprites(Ts.Get(TextureStrings.NightmareSparkKey));

            var item = new ItemChanger.Items.CharmItem()
            {
                charmNum = CharmIDs[0],
                name = _charmNames[0],
                UIDef = new MsgUIDef()
                {
                    name = new LanguageString("UI", $"CHARM_NAME_{CharmIDs[0]}"),
                    shopDesc = new LanguageString("UI", $"CHARM_DESC_{CharmIDs[0]}"),
                    sprite = new ICSprite()
                }
            };
            // Tag the item for ConnectionMetadataInjector, so that MapModS and
            // other mods recognize the items we're adding as charms.
            var mapmodTag = item.AddTag<InteropTag>();
            mapmodTag.Message = "RandoSupplementalMetadata";
            mapmodTag.Properties["ModSource"] = GetName();
            mapmodTag.Properties["PoolGroup"] = "Charms";
            Finder.DefineCustomItem(item);

            InitCallbacks();
            On.PlayMakerFSM.Awake += FSMAwake;
            ModHooks.DashPressedHook += Firetrail.StartTrail;
            ModHooks.SetPlayerBoolHook += CheckCharms;
            On.HealthManager.TakeDamage += Firebat.BatDie;
            ModHooks.HeroUpdateHook += ShapeOfGrimm.GrimmSlugMovement;
            ModHooks.FinishedLoadingModsHook += DebugGiveCharm;
            On.UIManager.StartNewGame += (On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush) =>
            {
                ItemChangerMod.CreateSettingsProfile(overwrite: false, createDefaultModules: false);
                orig(self, permaDeath, bossRush);
            };
            ModHooks.SetPlayerBoolHook += (string target, bool orig) =>
            {
                var pd = PlayerData.instance;
                if (pd.GetBool("bossRushMode"))
                {
                    SaveSettings.gotCharms[0] = true;
                }
                if (!Placed && !pd.GetBool($"gotCharm_{CharmIDs[0]}") && !pd.GetBool("troupeInTown") && pd.GetBool("destroyedNightmareLantern") || !Placed && !pd.GetBool($"gotCharm_{CharmIDs[0]}") && !pd.GetBool("troupeInTown") && pd.GetBool("killedNightmareGrimm"))
                {
                    float xpos = 47.2f;
                    float ypos = 4.4f;
                    var placements = new List<AbstractPlacement>();
                    var name = _charmNames[0];
                    placements.Add(
                        new CoordinateLocation()
                        {
                            x = xpos,
                            y = ypos,
                            elevation = 0,
                            sceneName = "Cliffs_06",
                            name = name
                        }
                        .Wrap()
                        .Add(Finder.GetItem(name)));
                    ItemChangerMod.AddPlacements(placements, conflictResolution: PlacementConflictResolution.Ignore);
                    Placed = true;
                }
                return orig;
            };
            Log("Initialized");
        }

        #region Charm Setup
        private void InitCallbacks()
        {
            ModHooks.GetPlayerBoolHook += OnGetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += OnGetPlayerIntHook;
            ModHooks.AfterSavegameLoadHook += InitSaveSettings;
            ModHooks.LanguageGetHook += OnLanguageGetHook;

        }
        private void InitSaveSettings(SaveGameData data)
        {
            // Found in a project, might help saving, don't know, but who cares
            // Charms
            SaveSettings.gotCharms = SaveSettings.gotCharms;
            SaveSettings.newCharms = SaveSettings.newCharms;
            SaveSettings.equippedCharms = SaveSettings.equippedCharms;
            SaveSettings.charmCosts = SaveSettings.charmCosts;
            SaveSettings.dP = SaveSettings.dP; //Dwarf Pogo :dwarfwoot:
        }

        private readonly string[] _charmNames =
        {
            "Nightmare Spark",

        };
        private readonly string[] _charmDescriptions =
        {
            "A remnant of the Nightmare King's power,<br>still resonating with the everburning fire of the Troupe.<br><br>Dashing leaves a fire trail behind which<br>can damage enemies",

        };
        private string OnLanguageGetHook(string key, string sheet, string orig)
        {
            if (key.StartsWith("CHARM_NAME_"))
            {
                int charmNum = int.Parse(key.Split('_')[2]);
                if (CharmIDs.Contains(charmNum))
                {
                    return _charmNames[CharmIDs.IndexOf(charmNum)];
                }
            }
            else if (key.StartsWith("CHARM_DESC_"))
            {
                int charmNum = int.Parse(key.Split('_')[2]);
                if (CharmIDs.Contains(charmNum))
                {
                    return _charmDescriptions[CharmIDs.IndexOf(charmNum)];
                }
            }
            return orig;
        }
        private bool OnGetPlayerBoolHook(string target, bool orig)
        {
            if (target.StartsWith("gotCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    return SaveSettings.gotCharms[CharmIDs.IndexOf(charmNum)];
                }
            }
            if (target.StartsWith("newCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    return SaveSettings.newCharms[CharmIDs.IndexOf(charmNum)];
                }
            }
            if (target.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    return SaveSettings.equippedCharms[CharmIDs.IndexOf(charmNum)];
                }
            }
            return orig;
        }
        private bool OnSetPlayerBoolHook(string target, bool orig)
        {
            if (target.StartsWith("gotCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    SaveSettings.gotCharms[CharmIDs.IndexOf(charmNum)] = orig;
                    return orig;
                }
            }
            if (target.StartsWith("newCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    SaveSettings.newCharms[CharmIDs.IndexOf(charmNum)] = orig;
                    return orig;
                }
            }
            if (target.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    SaveSettings.equippedCharms[CharmIDs.IndexOf(charmNum)] = orig;
                    return orig;
                }
            }
            return orig;
        }
        private int OnGetPlayerIntHook(string target, int orig)
        {
            if (target.StartsWith("charmCost_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (CharmIDs.Contains(charmNum))
                {
                    return SaveSettings.charmCosts[CharmIDs.IndexOf(charmNum)];
                }
            }
            return orig;
        }

        #endregion



        private void FSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName == "Control")
            {
                if (self.gameObject.name == "Grimmchild(Clone)")
                {
                    FsmState grimmchild = self.GetState("Antic");
                    grimmchild.InsertCustomAction("Antic", () => Grimmchild.GrimmchildMain(), 7);
                }
                
            }
            if (self.FsmName == "Spell Control")
            {
                FsmState castShadeSoul = self.GetState("Fireball 2");
                castShadeSoul.InsertCustomAction("Fireball 2", () => Firebat.SpawnBat(20), 4);
                FsmState castVengefulSpirit = self.GetState("Fireball 1");
                castVengefulSpirit.InsertCustomAction("Fireball 1", () => Firebat.SpawnBat(15), 4);
                FsmState castQuakeDive = self.GetState("Q1 Effect");
                castQuakeDive.InsertCustomAction("Q1 Effect", () => DiveFireball.DiveFireballs(15, 24), 4);
                FsmState castQuakeDark = self.GetState("Q2 Effect");
                castQuakeDark.InsertCustomAction("Q2 Effect", () => DiveFireball.DiveFireballs(20, 36), 4);
                FsmState castSlug = self.GetState("Focus S");
                castSlug.InsertCustomAction("Focus S", () => ShapeOfGrimm.GrimmSlug(), 15);
            }
        }
        private bool CheckCharms(string target, bool orig)
        {
           
            //--------Fireball Dive--------//

            if (HeroController.instance == null || HeroController.instance.spellControl == null) { return orig; }
            FsmState castQuakeDive = HeroController.instance.spellControl.GetState("Q1 Effect");
            FsmState castQuakeDark = HeroController.instance.spellControl.GetState("Q2 Effect");
            if (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}") && PlayerData.instance.GetBool("equippedCharm_37"))
            {
                castQuakeDive.GetAction<CustomFsmAction>(4).Enabled = true;
                castQuakeDark.GetAction<CustomFsmAction>(4).Enabled = true;
            }
            else
            {
                castQuakeDive.GetAction<CustomFsmAction>(4).Enabled = false;
                castQuakeDark.GetAction<CustomFsmAction>(4).Enabled = false;
            }

            //--------Firebat Spell--------//

            FsmState castShadeSoul = HeroController.instance.spellControl.GetState("Fireball 2");
            FsmState castVengefulSpirit = HeroController.instance.spellControl.GetState("Fireball 1");
            if (PlayerData.instance.GetBool("equippedCharm_11") && PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}"))
            {
                castShadeSoul.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = false;
                castShadeSoul.GetAction<CustomFsmAction>(4).Enabled = true;
                castVengefulSpirit.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = false;
                castVengefulSpirit.GetAction<CustomFsmAction>(4).Enabled = true;


            }
            else
            {
                castShadeSoul.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castShadeSoul.GetAction<CustomFsmAction>(4).Enabled = false;
                castVengefulSpirit.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castVengefulSpirit.GetAction<CustomFsmAction>(4).Enabled = false;
            }

            //--------Grimmchild--------//

            int gcLevel = PlayerData.instance.GetInt("grimmChildLevel");
            if (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}") && PlayerData.instance.GetBool("equippedCharm_40") && gcLevel <= 4 && gcLevel > 1)
            {
                var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
                PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
                
                if (grimmchild != null)
                {
                    gc.FsmVariables.FindFsmGameObject("Child").Value.Find("Enemy Range").transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    grimmchild.GetState("Antic").GetAction<CustomFsmAction>(7).Enabled = true;
                }

            }
            else
            {
                var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
                PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
                if (grimmchild != null)
                {
                    grimmchild.GetState("Antic").GetAction<CustomFsmAction>(7).Enabled = false;
                }
            }


            //--------Carefree/Thorns--------//
            if (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}") && gcLevel == 5 && PlayerData.instance.GetBool("equippedCharm_40"))
            {
                if (PlayerData.instance.GetBool("equippedCharm_12"))
                {
                    CarefreeSpikes.NightmareSpikeActivate();
                }
                else
                {
                    CarefreeSpikes.GrimmSpikeActivate();
                }

            }

            //--------Grimm Slug--------//
            var sc = HeroController.instance.spellControl;
            FsmState castSlug = HeroController.instance.spellControl.GetState("Focus S");
            if (PlayerData.instance.GetBool("equippedCharm_28") && PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}"))
            {
                castSlug.GetAction<CustomFsmAction>(15).Enabled = true;           
            }
            else
            {
                castSlug.GetAction<CustomFsmAction>(15).Enabled = false;
            }
            return orig;

        }


        public class ICSprite : ISprite
        {
            public Sprite Value { get; } = Ts.Get(TextureStrings.NightmareSparkKey);
            public ISprite Clone() => (ISprite)MemberwiseClone();
        }
        private void DebugGiveCharm()
        {
            if (ModHooks.GetMod("DebugMod") is Mod)

            {
                var commands = Type.GetType("DebugMod.BindableFunctions, DebugMod");
                if (commands == null)
                {
                    return;
                }
                var method = commands.GetMethod("GiveAllCharms", BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    return;
                }
                new Hook(
                    method,
                    (Action orig) =>
                    {
                        SaveSettings.gotCharms[0] = true;
                        orig();
                        
                    }
                );
            }
        }

        public static DamageEnemies AddDamageEnemy(GameObject go)
        {
            var dmg = go.GetAddComponent<DamageEnemies>();
            dmg.attackType = AttackTypes.Spell;
            dmg.circleDirection = false;
            dmg.damageDealt = 15;
            dmg.direction = 90 * 3;
            dmg.ignoreInvuln = false;
            dmg.magnitudeMult = 1f;
            dmg.moveDirection = false;
            dmg.specialType = 0;
            return dmg;
        }
    }
}