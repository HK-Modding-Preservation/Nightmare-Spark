using System;
using System.Collections;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel;
using Satchel.Futils;
using GlobalEnums;
using SFCore;
using SFCore.Generics;

namespace Nightmare_Spark
{
    public class Nightmare_Spark : SaveSettingsMod<SaveSettings>
    {
        new public string GetName() => "NightmareSpark";
        public override string GetVersion() => "V0.3";
        public Nightmare_Spark() : base("Nightmare Spark")
        {
            Ts = new TextureStrings();

        }
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>()
            {
                ("GG_Grimm_Nightmare", "Grimm Control/Nightmare Grimm Boss")
            };
        }
        public TextureStrings Ts { get; private set; }
        public List<int> CharmIDs { get; private set; }

        private GameObject? MyTrail;
        private GameObject? NKG;
        public static AudioSource AudioSource;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            NKG = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Control/Nightmare Grimm Boss"];
            GameObject.DontDestroyOnLoad(NKG);

            var go = new GameObject("AudioSource");
            AudioSource = go.AddComponent<AudioSource>();
            AudioSource.pitch = 1f;
            AudioSource.volume = 1f;
            UnityEngine.Object.DontDestroyOnLoad(AudioSource);

            CharmIDs = CharmHelper.AddSprites(Ts.Get(TextureStrings.NightmareSparkKey));

            InitCallbacks();
            On.PlayMakerFSM.Awake += FSMAwake;
            ModHooks.DashPressedHook += StartTrail;
            ModHooks.SetPlayerBoolHook += CheckCharms;
            Log("Initialized");
        }

        private void FSMAwake(On.PlayMakerFSM.orig_Awake orig, PlayMakerFSM self)
        {
            orig(self);  
            if (self.FsmName == "Spell Control")
            {
                FsmState castShadeSoul = self.GetState("Fireball 2");
                castShadeSoul.InsertCustomAction("Fireball 2", () => SpawnBat(15), 4);
                FsmState castVengefulSpirit = self.GetState("Fireball 1");
                castVengefulSpirit.InsertCustomAction("Fireball 1", () => SpawnBat(10), 4);
            }
        }

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
        }

        private string[] _charmNames =
        {
            "Nightmare Spark",
            
        };
        private string[] _charmDescriptions =
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


        public class MyMonoBehaviourForBats : MonoBehaviour
        {
            Rigidbody2D? rb2d;


            void Awake()
            {
                if (!HeroController.instance)
                {
                    return;
                }
                 
            }
            void Start()
            {
                GameObject Firebat = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);

                if (!HeroController.instance)
                {
                    return;
                }
                var audioClip = Firebat.GetState("Firebat 1").GetAction<AudioPlayerOneShotSingle>(8).audioClip.Value as AudioClip;
                Nightmare_Spark.AudioSource.PlayOneShot(audioClip);

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                var facing = HeroController.instance.cState.facingRight;
                rb2d.velocity = new Vector2(facing ? 25f : -25f, 0f);
                var oldscale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(oldscale.x * (facing ? .75f : -.75f), .75f, oldscale.z);

            }
            void OnDestroy()
            {

            }
        }


        private int gcdamage;
        private bool CheckCharms(string target, bool orig)
        {

            if (HeroController.instance == null || HeroController.instance.spellControl == null) { return orig; }
            FsmState castShadeSoul = HeroController.instance.spellControl.GetState("Fireball 2");
            FsmState castVengefulSpirit = HeroController.instance.spellControl.GetState("Fireball 1");
            if (PlayerData.instance.GetBool("equippedCharm_11") && PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}")) 
            {
                castShadeSoul.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = false;
                castShadeSoul.GetAction<CustomFsmAction>(4).Enabled = true;
                castVengefulSpirit.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = false;
                castVengefulSpirit.GetAction<CustomFsmAction>(4).Enabled = true;
                int gcLevel = PlayerData.instance.GetInt("grimmChildLevel");
                if (PlayerData.instance.GetBool("equippedCharm_40") && gcLevel <= 4)
                {


                    gcdamage = gcLevel switch
                    {
                        2 => (int)(5 * 1.5f),
                        3 => (int)(8 * 1.5f),
                        4 => (int)(11 * 1.5f)


                    };

                    
                    var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
                    PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
                    grimmchild.GetState("Shoot").GetAction<SetFsmInt>(6).setValue = gcdamage;
                }
            }
            else
            {
                castShadeSoul.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castShadeSoul.GetAction<CustomFsmAction>(4).Enabled = false;
                castVengefulSpirit.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castVengefulSpirit.GetAction<CustomFsmAction>(4).Enabled = false;
            }

            return orig;

        }
 

        private string SpawnBat(int spellLevel)
        {
            
            GameManager.instance.StartCoroutine(BatCoroutine(spellLevel));  
            return "SendMessage";
        }

        private IEnumerator BatCoroutine(int damage)
        {
            yield return new WaitWhile(() => HeroController.instance == null);
            for (int i = 0; i <= 2; i++)
            {

                GameObject Firebat = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                GameObject.Destroy(Firebat.LocateMyFSM("Control"));
                Firebat.AddComponent<MyMonoBehaviourForBats>();
                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    AddDamageEnemy(Firebat).damageDealt = (int)(damage * 1.5f);
                }
                else
                {
                    AddDamageEnemy(Firebat).damageDealt = damage;
                }

                foreach (var DH in Firebat.GetComponentsInChildren<DamageHero>())
                {
                    GameObject.Destroy(DH);
                }
                Firebat.layer = (int)PhysLayers.HERO_ATTACK;
                GameObject.DontDestroyOnLoad(Firebat);
                var col = Firebat.GetComponent<Collider2D>();

                col.isTrigger = true;


                Firebat.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5f, 0);
                yield return new WaitForSeconds(.25F);
            } 
        }

        private readonly int numberOfSpawns = 8;
        private readonly float Rate = 15f;
        private IEnumerator MyCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance == null);


            for (var i = 0; i < numberOfSpawns; i++)
            {
                MyTrail = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("AD Fire").GetAction<SpawnObjectFromGlobalPoolOverTime>(7).gameObject.Value);
                GameObject.DontDestroyOnLoad(MyTrail);
                UnityEngine.Object.Destroy(MyTrail.LocateMyFSM("damages_hero"));
                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    AddDamageEnemy(MyTrail).damageDealt = 20;
                }
                else
                {
                    AddDamageEnemy(MyTrail);
                }
                
                MyTrail.gameObject.GetComponent<ParticleSystem>().startSize = 0.25F;
                MyTrail.layer = (int)PhysLayers.HERO_ATTACK;

                //Instantiates here
                UnityEngine.Object.Instantiate(MyTrail);
                //Delay at 1f/rate
                MyTrail.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5F, -0.03f);
                yield return new WaitForSeconds(1f / Rate);

            }
        }

        private bool StartTrail()
        {
            if (Satchel.Reflected.HeroControllerR.CanDash() == true && (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}")))
            {
                    GameManager.instance.StartCoroutine(MyCoroutine());
                    return false;

            }
            else
            {
                return false;
            }
        }
        public DamageEnemies AddDamageEnemy(GameObject go)
        {
            var dmg = go.GetAddComponent<DamageEnemies>();
            dmg.attackType = AttackTypes.Nail;
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