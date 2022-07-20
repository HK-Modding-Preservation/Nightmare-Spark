using Modding;
namespace Nightmare_Spark
{
    internal class Firebat
    {
        private static GameObject nkg = Nightmare_Spark.nkg;
        public static string SpawnBat(int spellLevel)
        {
            if (PlayerData.instance.GetBool("equippedCharm_10"))
            {
                GameObject firebat = GameObject.Instantiate(nkg.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                GameObject.Destroy(firebat.LocateMyFSM("Control"));
                firebat.layer = (int)PhysLayers.HERO_ATTACK;
                GameObject.Destroy(firebat.Find("Flash Damage"));
                firebat.Find("Hero Hurter").active = false;
                firebat.GetComponent<CircleCollider2D>().isTrigger = true;
                Nightmare_Spark.AddDamageEnemy(firebat).damageDealt = (int)(spellLevel * 3.5f);

                foreach (var DH in firebat.GetComponentsInChildren<DamageHero>())
                {
                    GameObject.Destroy(DH);
                }
                firebat.AddComponent<NonBouncer>();
                firebat.AddComponent<MonoBehaviourForBigBat>();
                firebat.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5f, 0);


                var burst = GameObject.Instantiate(nkg.LocateMyFSM("Control").GetState("AD Fire").GetAction<SpawnObjectFromGlobalPoolOverTime>(7).gameObject.Value);
                burst.name = "Burst";
                burst.active = false;
                burst.transform.parent = firebat.transform;
                GameObject.Destroy(burst.LocateMyFSM("damages_hero"));
                Nightmare_Spark.AddDamageEnemy(burst).damageDealt = 10;
                burst.gameObject.GetComponent<ParticleSystem>().startSize = 200;

                burst.gameObject.SetScale(1.75f, 1.75f);
                burst.layer = (int)PhysLayers.HERO_ATTACK;
                burst.AddComponent<NonBouncer>();

                burst.transform.position = firebat.transform.position - new Vector3(0, 0, 0);
               
            }
            else
            {
                GameManager.instance.StartCoroutine(BatCoroutine(spellLevel));
            }
            return "SendMessage";
        }

        private static IEnumerator BatCoroutine(int damage)
        {
            yield return new WaitWhile(() => HeroController.instance == null);
            for (int i = 0; i <= 2; i++)
            {

                GameObject firebat = GameObject.Instantiate(nkg.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value);
                firebat.AddComponent<MyMonoBehaviourForBats>();
                GameObject.Destroy(firebat.LocateMyFSM("Control"));
                firebat.layer = (int)PhysLayers.HERO_ATTACK;
                firebat.GetComponent<CircleCollider2D>().isTrigger = true;
                GameObject.Destroy(firebat.Find("Flash Damage"));
                firebat.Find("Hero Hurter").active = false;
                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    Nightmare_Spark.AddDamageEnemy(firebat).damageDealt = (int)(damage * 1.5f);
                }
                else
                {
                    Nightmare_Spark.AddDamageEnemy(firebat).damageDealt = damage;
                }

                foreach (var DH in firebat.GetComponentsInChildren<DamageHero>())
                {
                    GameObject.Destroy(DH);
                }

                //  GameObject.DontDestroyOnLoad(Firebat);

                firebat.AddComponent<NonBouncer>();
                var facing = HeroController.instance.cState.facingRight;
                float x = facing ? -.5f : .5f;
                firebat.transform.position = HeroController.instance.transform.position - new Vector3(x, 0.5f, 0);
                yield return new WaitForSeconds(.25F);
            }
        }
        public static void BatDie(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (hitInstance.Source.GetComponent<MyMonoBehaviourForBats>() != null)
            {
                hitInstance.Source.transform.Find("Impact").gameObject.active = true;
                //gameObject.GetComponent<tk2dSpriteAnimator>().CurrentClip.name = "Impact";
                //gameObject.GetComponent<tk2dSpriteAnimator>().Play();
                hitInstance.Source.GetComponent<tk2dSpriteAnimator>().Play("Impact");
                GameManager.instance.StartCoroutine(DestroyBat(hitInstance.Source));
                //AnimationUtils.logTk2dAnimationClips(hitInstance.Source.Find("Impact"));
            }
            if (hitInstance.Source.GetComponent<MonoBehaviourForBigBat>() != null)
            {
                hitInstance.Source.Find("Burst").active = true;
                hitInstance.Source.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                hitInstance.Source.GetComponent<MeshRenderer>().enabled = false;
                hitInstance.Source.GetComponent<CircleCollider2D>().enabled = false;
                //hitInstance.Source.transform.Find("Impact").gameObject.active = true;
                //hitInstance.Source.GetComponent<tk2dSpriteAnimator>().Play("Impact");
                GameManager.instance.StartCoroutine(Wait(hitInstance.Source));
                
            }
            orig(self, hitInstance);
        }
        private static IEnumerator Wait(GameObject go)
        {
            yield return new WaitUntil(() => !go.Find("Burst").GetComponent<ParticleSystem>().isPlaying);
            GameObject.Destroy(go);
        }
        private static IEnumerator DestroyBat(GameObject go)
        {
            Rigidbody2D rb2d;
            rb2d = go.GetAddComponent<Rigidbody2D>();
            rb2d.velocity = new Vector3(0, 0, 0);
            var facing = HeroController.instance.cState.facingRight;
            go.transform.Find("Impact").GetComponent<Transform>().localPosition = new Vector3(-1.5f, 0.01f, -1f);
            //gameObject.GetComponent<ParticleSystem>().Play();
            go.GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(0.1f);
            GameObject.Destroy(go);
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
                var firebat = nkg.LocateMyFSM("Control").GetState("Firebat 1");

                if (!HeroController.instance)
                {
                    return;
                }
                Nightmare_Spark.audioSource.pitch = .75f;
                Nightmare_Spark.audioSource.volume = .05f;
                var audioClip = firebat.GetAction<AudioPlayerOneShotSingle>(8).audioClip.Value as AudioClip;
                Nightmare_Spark.audioSource.PlayOneShot(audioClip);

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                var facing = HeroController.instance.cState.facingRight;
                rb2d.velocity = new Vector2(facing ? 25f : -25f, 0f);
                var oldscale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(oldscale.x * (facing ? .75f : -.75f), .75f, oldscale.z);

                gameObject.transform.Find("Flash Damage").gameObject.active = false;
            }
            public void OnTriggerEnter2D(Collider2D collision)
            {
                if (collision.gameObject.layer == (int)PhysLayers.TERRAIN)
                {
                    gameObject.transform.Find("Impact").gameObject.active = true;

                    GameManager.instance.StartCoroutine(Destroy());

                }
            }

            private IEnumerator Destroy()
            {

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                rb2d.velocity = new Vector3(0, 0, 0);
                var facing = HeroController.instance.cState.facingRight;
                gameObject.transform.Find("Impact").GetComponent<Transform>().localPosition = new Vector3(-1.5f, 0.01f, -1f);
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                var firebat = nkg.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value;
                var impactclip = firebat.LocateMyFSM("Control").GetState("Impact").GetAction<Tk2dPlayAnimationWithEvents>(11).clipName.Value;
                gameObject.Find("Impact").GetComponent<tk2dSpriteAnimator>().Play(impactclip);
                yield return new WaitForSeconds(0.0738f);
                Destroy(gameObject);
            }

            void OnDestroy()
            {
                var impact = nkg.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value.LocateMyFSM("Control").GetState("Impact");
                var audioClip = impact.GetAction<AudioPlaySimple>(1).oneShotClip.Value as AudioClip;
                Nightmare_Spark.audioSource.PlayOneShot(audioClip);
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

                var firebat = nkg.LocateMyFSM("Control").GetState("Firebat 1");

                if (!HeroController.instance)
                {
                    return;
                }
                Nightmare_Spark.audioSource.pitch = .75f;
                Nightmare_Spark.audioSource.volume = .05f;
                var audioClip = firebat.GetAction<AudioPlayerOneShotSingle>(8).audioClip.Value as AudioClip;
                Nightmare_Spark.audioSource.PlayOneShot(audioClip);
                gameObject.transform.Find("Hero Hurter").gameObject.active = false;

                rb2d = gameObject.GetAddComponent<Rigidbody2D>();
                var facing = HeroController.instance.cState.facingRight;
                rb2d.velocity = new Vector2(facing ? 15f : -15f, 0f);
                var oldscale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(oldscale.x * (facing ? 2f : -2f), 2f, oldscale.z);
            }
            void OnDestroy()
            { 
                var impact = nkg.LocateMyFSM("Control").GetState("Firebat 1").GetAction<SpawnObjectFromGlobalPool>(2).gameObject.Value.LocateMyFSM("Control").GetState("Impact");
                var audioClip = impact.GetAction<AudioPlaySimple>(1).oneShotClip.Value as AudioClip;
                Nightmare_Spark.audioSource.PlayOneShot(audioClip);
                gameObject.transform.Find("Impact").gameObject.active = true;
                gameObject.GetComponent<tk2dSpriteAnimator>().Play("Impact");
            }
        }
    }
}
