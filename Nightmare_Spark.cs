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
using ItemChanger;
using System.Reflection;
using InControl;

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
        private static GameObject? NKG;
        public static AudioSource AudioSource;
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            NKG = preloadedObjects["GG_Grimm_Nightmare"]["Grimm Control/Nightmare Grimm Boss"];
            GameObject.DontDestroyOnLoad(NKG);

            var go = new GameObject("AudioSource");
            AudioSource = go.AddComponent<AudioSource>();
            AudioSource.pitch = .75f;
            AudioSource.volume = .3f;
            UnityEngine.Object.DontDestroyOnLoad(AudioSource);

            CharmIDs = CharmHelper.AddSprites(Ts.Get(TextureStrings.NightmareSparkKey));

            InitCallbacks();
            On.PlayMakerFSM.Awake += FSMAwake;
            ModHooks.DashPressedHook += StartTrail;
            ModHooks.SetPlayerBoolHook += CheckCharms;
            On.HealthManager.TakeDamage += BatDie;
            ModHooks.HeroUpdateHook += GrimmSlugMovement;
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
                FsmState castQuakeDive = self.GetState("Q1 Effect");
                castQuakeDive.InsertCustomAction("Q1 Effect", () => DiveFireballs(15, 24), 4);
                FsmState castQuakeDark = self.GetState("Q2 Effect");
                castQuakeDark.InsertCustomAction("Q2 Effect", () => DiveFireballs(20, 36), 4);
                FsmState castSlug = self.GetState("Focus S");
                castSlug.InsertCustomAction("Focus S", () => GrimmSlug(), 15);
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
            SaveSettings.dwarfPogo = SaveSettings.dwarfPogo;
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
                var Firebat = NKG.LocateMyFSM("Control").GetState("Firebat 1");

                if (!HeroController.instance)
                {
                    return;
                }
                Nightmare_Spark.AudioSource.pitch = .75f;
                Nightmare_Spark.AudioSource.volume = .3f;
                var audioClip = Firebat.GetAction<AudioPlayerOneShotSingle>(8).audioClip.Value as AudioClip;
                Nightmare_Spark.AudioSource.PlayOneShot(audioClip);

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                var facing = HeroController.instance.cState.facingRight;
                rb2d.velocity = new Vector2(facing ? 25f : -25f, 0f);
                var oldscale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(oldscale.x * (facing ? .75f : -.75f), .75f, oldscale.z);

                //GameObject.Destroy(gameObject.transform.GetChild(3).gameObject);
            }
            public void OnTriggerStay2D(Collider2D collision)
            {
                if (collision.gameObject.layer == (int)PhysLayers.TERRAIN)
                {
                    var impact = NKG.LocateMyFSM("Control").GetState("Impact");
                    var audioClip = impact.GetAction<AudioPlaySimple>(1).oneShotClip.Value as AudioClip;
                    Nightmare_Spark.AudioSource.PlayOneShot(audioClip);

                    Destroy(gameObject);
                }
            }
            void OnDestroy()
            {
                //GameObject Firebat = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                gameObject.LocateMyFSM("Control").GetState("Impact").GetAction<PlayParticleEmitter>(5).emit.Value = 1;
                var impact = gameObject.LocateMyFSM("Control").GetState("Impact").GetAction<PlayParticleEmitter>(5);

            }
        }
        public class MonoBehaviourForBigBat : MonoBehaviour
        {
            Rigidbody2D? rb2d;
            void Awake()
            {

            }
            void Start()
            {

                var Firebat = NKG.LocateMyFSM("Control").GetState("Firebat 1");

                if (!HeroController.instance)
                {
                    return;
                }
                Nightmare_Spark.AudioSource.pitch = .75f;
                Nightmare_Spark.AudioSource.volume = .3f;
                var audioClip = Firebat.GetAction<AudioPlayerOneShotSingle>(8).audioClip.Value as AudioClip;
                Nightmare_Spark.AudioSource.PlayOneShot(audioClip);

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                var facing = HeroController.instance.cState.facingRight;
                rb2d.velocity = new Vector2(facing ? 15f : -15f, 0f);
                var oldscale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(oldscale.x * (facing ? 2f : -2f), 2f, oldscale.z);
            }
            void OnDestroy()
            {

            }
        }

        private int gcdamage;
        private bool CheckCharms(string target, bool orig)
        {
            if (HeroController.instance == null || HeroController.instance.spellControl == null) { return orig; }
            FsmState castQuakeDive = HeroController.instance.spellControl.GetState("Q1 Effect");
            FsmState castQuakeDark = HeroController.instance.spellControl.GetState("Q2 Effect");
            if (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}"))
            {
                castQuakeDive.GetAction<CustomFsmAction>(4).Enabled = true;
                castQuakeDark.GetAction<CustomFsmAction>(4).Enabled = true;
            }
            else
            {
                castQuakeDive.GetAction<CustomFsmAction>(4).Enabled = false;
                castQuakeDark.GetAction<CustomFsmAction>(4).Enabled = false;
            }

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
                    if (grimmchild != null)
                    { grimmchild.GetState("Shoot").GetAction<SetFsmInt>(6).setValue = gcdamage; }
                }
            }
            else
            {
                castShadeSoul.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castShadeSoul.GetAction<CustomFsmAction>(4).Enabled = false;
                castVengefulSpirit.GetAction<SpawnObjectFromGlobalPool>(3).Enabled = true;
                castVengefulSpirit.GetAction<CustomFsmAction>(4).Enabled = false;
            }
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

        private readonly FieldInfo[] heroActionFields = typeof(HeroActions).GetFields(BindingFlags.Instance | BindingFlags.Public);
        public bool gsActive = false;


        private void GrimmSlugMovement()
        {
            if (gsActive)
            {
                int gsvertical = 0;
                int gshorizontal = 0;
                foreach (var heroActionField in heroActionFields)
                {
                    string actionName = heroActionField.Name;
                    if (heroActionField.GetValue(InputHandler.Instance.inputActions) is PlayerAction playerAction)
                    {
                        if (playerAction.IsPressed)
                        {
                            for (int i = 0; i < MyVars.strActions.Length; i++)
                            {

                                if (MyVars.strActions[i] == actionName)
                                {
                                    switch (actionName)
                                    {
                                        case "up":
                                            gsvertical = 5;
                                            break;
                                        case "down":
                                            gsvertical = -5;
                                            break;
                                        case "right":
                                            gshorizontal = 5;
                                            break;
                                        case "left":
                                            gshorizontal = -5;
                                            break;
                                        default:
                                            gshorizontal = 0;
                                            gsvertical = 0;
                                            break;


                                    };
                                    HeroController.instance.GetComponent<Rigidbody2D>().velocity = new Vector2(gshorizontal, gsvertical);

                                }
                            }
                        }
                    }
                }
            }
            var sc = HeroController.instance.spellControl;
            if (gsActive && !sc.GetState("Focus Cancel 2").GetAction<SetBoolValue>(16).boolVariable.Value || gsActive && !sc.GetState("Focus Get Finish 2").GetAction<SetBoolValue>(15).boolVariable.Value)
            {
                gsActive = false;

                sc.AddTransition("Focus S", "LEFT GROUND", "Grace Check 2");
                sc.AddTransition("Focus Left", "LEFT GROUND", "Grace Check 2");
                sc.AddTransition("Focus Right", "LEFT GROUND", "Grace Check 2");

                HeroController.instance.AffectedByGravity(true);


            }
        }

        [RequireComponent(typeof(LineRenderer))]
        public class Circle : MonoBehaviour
        {
            [Range(0, 55)]
            public int segments = 55;
            [Range(0, 5)]
            public float xradius = 5;
            [Range(0, 5)]
            public float yradius = 5;
            LineRenderer limit;
            private void Start()
            {
                limit = gameObject.GetComponent<LineRenderer>();
                limit.name = "Limit";
                limit.startWidth = .5f;
                limit.endWidth = .5f;
                limit.startColor = new Color32(1, 0, 0, 1);
                limit.endColor = new Color32(1, 0, 0, 1);
                limit.SetVertexCount(segments + 1);
                limit.useWorldSpace = false;
                CreatePoints();

            }
            void CreatePoints()
            {
                float x;
                float y;
                float z;

                float angle = 20f;

                for (int i = 0; i < (segments + 1); i++)
                {
                    x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
                    y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

                    limit.SetPosition(i, new Vector3(x, y, 0));

                    angle += (360f / segments);
                }
            }
        }
        private void GrimmSlug()
        {
            var sc = HeroController.instance.spellControl;

            HeroController.instance.AffectedByGravity(false);

            sc.RemoveTransition("Focus S", "LEFT GROUND");
            sc.RemoveTransition("Focus Left", "LEFT GROUND");
            sc.RemoveTransition("Focus Right", "LEFT GROUND");

            gsActive = true;

            GameObject go = new GameObject();
            go.name = "go";
            go.AddComponent<Circle>();
            GameObject.Instantiate(go).transform.position = HeroController.instance.transform.position - new Vector3(0, 0, 0);
            GameManager.instance.StartCoroutine(Timer(go));

        }
        private IEnumerator Timer(GameObject go)
        {
            yield return new WaitWhile(() => HeroController.instance.spellControl.GetState("Focus Cancel 2").GetAction<SetBoolValue>(16).boolVariable.Value || HeroController.instance.spellControl.GetState("Focus Get Finished 2").GetAction<SetBoolValue>(15).boolVariable.Value)
            {
                
            };
            GameObject.Destroy(go);

        }
        private void DiveFireballs(int damage, int spread)
        {
            int x = spread;
            for (int i = -x; i <= x; i += 12)
            {
                var Fireball = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("UP Explode").GetAction<SpawnObjectFromGlobalPool>(10).gameObject.Value);
                Fireball.RemoveComponent<DamageHero>();
                AddDamageEnemy(Fireball).damageDealt = damage;
                Fireball.layer = (int)PhysLayers.HERO_ATTACK;
                GameObject.DontDestroyOnLoad(Fireball);
                var rb2d = Fireball.GetComponent<Rigidbody2D>();
                rb2d.velocity = new Vector2(i, 1);
                Fireball.transform.position = HeroController.instance.transform.position - new Vector3(0, 0, 0);
            }


        }

        private string SpawnBat(int spellLevel)
        {
            if (PlayerData.instance.GetBool("equippedCharm_10"))
            {
                GameObject Firebat = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                GameObject.Destroy(Firebat.LocateMyFSM("Control"));
                Firebat.layer = (int)PhysLayers.HERO_ATTACK;
                var col = Firebat.GetComponent<Collider2D>();
                col.enabled = true;
                col.isTrigger = true;
                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    AddDamageEnemy(Firebat).damageDealt = (int)((spellLevel * 3) * 1.5f);
                }
                else
                {
                    AddDamageEnemy(Firebat).damageDealt = (int)(spellLevel * 3f);
                }
                foreach (var DH in Firebat.GetComponentsInChildren<DamageHero>())
                {
                    GameObject.Destroy(DH);
                }
                
                Firebat.AddComponent<MonoBehaviourForBigBat>();
                GameObject.DontDestroyOnLoad(Firebat);
                Firebat.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5f, 0);
            }
            else
            {
                GameManager.instance.StartCoroutine(BatCoroutine(spellLevel));
            }
            return "SendMessage";
        }

        private IEnumerator BatCoroutine(int damage)
        {
            yield return new WaitWhile(() => HeroController.instance == null);
            for (int i = 0; i <= 2; i++)
            {

                GameObject Firebat = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                GameObject.Destroy(Firebat.LocateMyFSM("Control"));
                Firebat.layer = (int)PhysLayers.HERO_ATTACK;
                var col = Firebat.GetComponent<Collider2D>();
                col.enabled = true;
                col.isTrigger = true;

                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    AddDamageEnemy(Firebat).damageDealt = (int)(damage * 1.5f);
                }
                else
                {
                    AddDamageEnemy(Firebat).damageDealt = damage;
                }
                Firebat.AddComponent<MyMonoBehaviourForBats>();
                foreach (var DH in Firebat.GetComponentsInChildren<DamageHero>())
                {
                    GameObject.Destroy(DH);
                }

                GameObject.DontDestroyOnLoad(Firebat);



                Firebat.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5f, 0);
                yield return new WaitForSeconds(.25F);
            }
        }
        private void BatDie(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (hitInstance.Source.GetComponent<MyMonoBehaviourForBats>() != null)
            {
                GameObject.Destroy(hitInstance.Source);
            }
            if (hitInstance.Source.GetComponent<MonoBehaviourForBigBat>() != null)
            {
                var Burst = GameObject.Instantiate(NKG.LocateMyFSM("Control").GetState("AD Fire").GetAction<SpawnObjectFromGlobalPoolOverTime>(7).gameObject.Value);
                GameObject.DontDestroyOnLoad(Burst);
                UnityEngine.Object.Destroy(Burst.GetComponentInChildren<DamageHero>());
                AddDamageEnemy(Burst).damageDealt = 10;
                Burst.gameObject.GetComponent<ParticleSystem>().startSize = 200;

                Burst.gameObject.SetScale(1.75f, 1.75f);
                Burst.layer = (int)PhysLayers.HERO_ATTACK;
                Burst.AddComponent<NonBouncer>();
                UnityEngine.Object.Instantiate(Burst);
                Burst.transform.position = hitInstance.Source.transform.position - new Vector3(0, 0, 0);
                GameObject.Destroy(hitInstance.Source);
            }
            orig(self, hitInstance);
        }

        private readonly int numberOfSpawns = 8;
        private readonly float Rate = 15f;
        private IEnumerator TrailCoroutine()
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
                if (!SaveSettings.dwarfPogo)
                {
                    MyTrail.AddComponent<NonBouncer>();
                }

                //Instantiates here
                UnityEngine.Object.Instantiate(MyTrail);
                //Delay at 1f/rate
                MyTrail.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5F, -0.03f);
                yield return new WaitForSeconds(1f / Rate);

            }
        }

        private bool cooldown = false;
        private IEnumerator TrailCooldown(float duration)
        {
            cooldown = true;
            yield return new WaitForSeconds(duration);
            HeroController.instance.GetComponent<SpriteFlash>().flash(new Color(1, 0, 0), 0.85f, 0.01f, 0.01f, 0.35f);

            cooldown = false;
        }
        private bool StartTrail()
        {
            if (Satchel.Reflected.HeroControllerR.CanDash() == true && (PlayerData.instance.GetBool($"equippedCharm_{CharmIDs[0]}")))
            {
                float duration;
                if (PlayerData.instance.GetBool("equippedCharm_31"))
                {
                    duration = 1f;
                }
                else
                {
                    duration = 1.75f;
                }

                if (!cooldown)
                {

                    GameManager.instance.StartCoroutine(TrailCoroutine());
                    GameManager.instance.StartCoroutine(TrailCooldown(duration));
                    return false;
                }
                else
                {

                    return false;
                }
            }
            else
            {
                return false;
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
        private static class MyVars
        {
            // Token: 0x04000003 RID: 3
            public static string vLastAction;

            // Token: 0x04000004 RID: 4
            public static string[] strActions = new string[]
            {
                "up",
                "down",
                "left",
                "right"
            };
        }
    }
}