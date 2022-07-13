namespace Nightmare_Spark
{
    
    internal class Firetrail
    {
        
        private static readonly int numberOfSpawns = 8;
        private static readonly float Rate = 15f;
        private static  bool cooldown = false;
        private static GameObject trail;
        public static bool StartTrail()
        {
            if (Satchel.Reflected.HeroControllerR.CanDash() == true && (PlayerData.instance.GetBool($"equippedCharm_{Nightmare_Spark.Instance.CharmIDs[0]}")))
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

        
        private static IEnumerator TrailCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance == null);


            for (var i = 0; i < numberOfSpawns; i++)
            {
                trail = GameObject.Instantiate(Nightmare_Spark.nkg.LocateMyFSM("Control").GetState("AD Fire").GetAction<SpawnObjectFromGlobalPoolOverTime>(7).gameObject.Value);
                GameObject.DontDestroyOnLoad(trail);
                UnityEngine.Object.Destroy(trail.LocateMyFSM("damages_hero"));
                if (PlayerData.instance.GetBool("equippedCharm_19"))
                {
                    Nightmare_Spark.AddDamageEnemy(trail).damageDealt = 20;
                }
                else
                {
                    Nightmare_Spark.AddDamageEnemy(trail);
                }

                trail.gameObject.GetComponent<ParticleSystem>().startSize = 0.25F;
                trail.layer = (int)PhysLayers.HERO_ATTACK;
                if (!SaveSettings.dP)
                {
                    trail.AddComponent<NonBouncer>();
                }

                //Instantiates here
                UnityEngine.Object.Instantiate(trail);
                //Delay at 1f/rate
                trail.transform.position = HeroController.instance.transform.position - new Vector3(0, 0.5F, -0.03f);
                yield return new WaitForSeconds(1f / Rate);

            }
        }
        private static IEnumerator TrailCooldown(float duration)
        {
            cooldown = true;
            yield return new WaitForSeconds(duration);
            HeroController.instance.GetComponent<SpriteFlash>().flash(new Color(1, 0, 0), 0.85f, 0.01f, 0.01f, 0.35f);

            cooldown = false;
        }
    }
}
