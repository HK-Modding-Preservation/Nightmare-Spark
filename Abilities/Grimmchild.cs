namespace Nightmare_Spark
{
    internal class Grimmchild
    {
        public static int shots = 0;
        public static bool Active = false;
        public static void GrimmchildMain()
        {     
            if (!Active)
            {
                var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
                PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
                GameObject grimmchildobj = gc.FsmVariables.FindFsmGameObject("Child").Value;
                Active = true;          
                shots++;
                if (shots == 1)
                {
                    var choice = 1;

                    var targetpos = grimmchild.GetState("Antic").GetAction<FaceObject>(1).objectB.Value.transform.position;
                    if ((targetpos - grimmchildobj.transform.position).sqrMagnitude >= 9*9)
                    {
                        //Far = Swirl
                        choice = 3;
                    }
                    if ((targetpos - grimmchildobj.transform.position).sqrMagnitude < 9*9 && (targetpos - grimmchildobj.transform.position).sqrMagnitude > 5*5)
                    {
                        //Mid = Burst
                        choice = 1;
                    }
                    if ((targetpos - grimmchildobj.transform.position).sqrMagnitude <= 5*5)
                    {
                        //Near = Pillar
                        choice = 2;
                    }

                    switch (choice)
                    {
                        case 1:
                            GrimmchildBurst();
                            break;
                        case 2:
                            GrimmchildPillar();
                            break;
                        case 3:
                            GrimmchildSwirl();
                            break;
                    }

                }              
                if (shots == 2)
                {
                    
                    grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 15;
                    var fireball = grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).storeObject.Value;
                    fireball.RemoveComponent<PillarBehaviour>();
                    fireball.Find("Enemy Damager").RemoveComponent<PillarChildBehaviour>();
                    GameManager.instance.StartCoroutine(WaitLikeHalfASecond());
                }
                if (shots == 3)
                {
                    grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 15;
                    var fireball = grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).storeObject.Value;
                    fireball.RemoveComponent<PillarBehaviour>();
                    fireball.Find("Enemy Damager").RemoveComponent<PillarChildBehaviour>();
                    shots = 0;
                    GameManager.instance.StartCoroutine(WaitLikeHalfASecond());
                }
            }
        }

        private static IEnumerator WaitLikeHalfASecond()
        {
            yield return new WaitForSeconds(.5f);
            Active = false;
        }

        private static void GrimmchildBurst()
        {
            int gcLevel = PlayerData.instance.GetInt("grimmChildLevel");
            Modding.Logger.Log("Picked Burst");
            if (gcLevel == 4)
            {
                GameManager.instance.StartCoroutine(FireBurstFive());
            }
            else
            {
                GameManager.instance.StartCoroutine(FireBurstThree());
                
            }
            GameManager.instance.StartCoroutine(WaitLikeHalfASecond());
        }
        private static void GrimmchildPillar()
        {
            Modding.Logger.Log("Picked Pillar");
            
            GameManager.instance.StartCoroutine(PillarCoroutine());    
            GameManager.instance.StartCoroutine(WaitLikeHalfASecond());
        }
        private static void GrimmchildSwirl()
        {
            Modding.Logger.Log("Picked Swirl");
            GameManager.instance.StartCoroutine(SwirlCoroutine());
            GameManager.instance.StartCoroutine(WaitLikeHalfASecond());
        }

        //-----Swirl Shot-----//
        private static IEnumerator SwirlCoroutine()
        {
            var grimmkinLarge = Nightmare_Spark.grimmkinSpawner.LocateMyFSM("Spawn Control").GetState("Level 3").GetAction<CreateObject>(0).gameObject.Value;
            var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
            GameObject grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value;

            yield return new WaitForSeconds(.25f);
            for (float i = 0; i <= 360; i += 90)
            {
                var flameball = GameObject.Instantiate(grimmkinLarge.LocateMyFSM("Control").GetState("Spiral Med").GetAction<SpawnObjectFromGlobalPool>(3).gameObject.Value);
                flameball.RemoveComponent<DamageHero>();
                Nightmare_Spark.AddDamageEnemy(flameball);
                flameball.layer = (int)PhysLayers.HERO_ATTACK;
                flameball.transform.position = grimmchild.transform.position;
                flameball.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Angle").Value = i;
                flameball.LocateMyFSM("Control").FsmVariables.FindFsmFloat("Accel").Value = .5f;
                flameball.LocateMyFSM("Control").SendEvent("SPIRAL");
            }

        }


        //-----Pillar Shot-----//
        public class PillarBehaviour : MonoBehaviour
        {
            private void Start()
            {
                gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            private void OnCollisionEnter2D(Collision2D collision)
            {
                if (collision.gameObject.layer == (int)PhysLayers.TERRAIN)
                {
                    var grimmkinLarge = Nightmare_Spark.grimmkinSpawner.LocateMyFSM("Spawn Control").GetState("Level 3").GetAction<CreateObject>(0).gameObject.Value;
                    var pillar = GameObject.Instantiate(grimmkinLarge.LocateMyFSM("Control").GetState("Spawn Pillar").GetAction<SpawnObjectFromGlobalPool>(0).gameObject.Value);
                    Nightmare_Spark.AddDamageEnemy(pillar);
                    pillar.layer = (int)PhysLayers.HERO_ATTACK;
                    pillar.RemoveComponent<DamageHero>();
                    GameObject.Destroy(pillar.Find("Pt Afterburn").LocateMyFSM("damages_hero"));
                    GameObject.Destroy(pillar.Find("Pillar").LocateMyFSM("damages_hero"));
                    pillar.transform.position = gameObject.transform.position - new Vector3(0, 1f, 0);
                    GameObject.Destroy(gameObject.GetComponent<PillarBehaviour>());
                    GameObject.Destroy(gameObject.Find("Enemy Damager").GetComponent<PillarChildBehaviour>());
                }
            }

        }
        public class PillarChildBehaviour : MonoBehaviour
        {
            private void Start()
            {
                gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (collision.gameObject.layer == (int)PhysLayers.ENEMIES)
                {
                    var impactpoint = gameObject.transform.position;
                    var grimmkinLarge = Nightmare_Spark.grimmkinSpawner.LocateMyFSM("Spawn Control").GetState("Level 3").GetAction<CreateObject>(0).gameObject.Value;
                    var pillar = GameObject.Instantiate(grimmkinLarge.LocateMyFSM("Control").GetState("Spawn Pillar").GetAction<SpawnObjectFromGlobalPool>(0).gameObject.Value);
                    Nightmare_Spark.AddDamageEnemy(pillar);
                    Nightmare_Spark.AddDamageEnemy(pillar.Find("Pillar"));
                    Nightmare_Spark.AddDamageEnemy(pillar.Find("Pt Afterburn"));
                    pillar.layer = (int)PhysLayers.HERO_ATTACK;
                    pillar.Find("Pillar").layer = (int)PhysLayers.HERO_ATTACK;
                    pillar.Find("Pt Afterburn").layer = (int)PhysLayers.HERO_ATTACK;
                    pillar.RemoveComponent<DamageHero>();
                    GameObject.Destroy(pillar.Find("Pt Afterburn").LocateMyFSM("damages_hero"));
                    GameObject.Destroy(pillar.Find("Pillar").LocateMyFSM("damages_hero"));
                    var floor = HeroController.instance.FindGroundPoint(impactpoint, false);
                    pillar.transform.position = floor - new Vector3(0, 1, 0);
                    GameObject.Destroy(gameObject.transform.parent.GetComponent<PillarBehaviour>());
                    GameObject.Destroy(gameObject.GetComponent<PillarChildBehaviour>());
                }
            }
        }

        private static IEnumerator PillarCoroutine()
        {
            var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
            PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
            yield return new WaitUntil(() => grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).storeObject.Value != null);
            var fireball = grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).storeObject.Value;
            fireball.AddComponent<PillarBehaviour>();
            fireball.Find("Enemy Damager").AddComponent<PillarChildBehaviour>();
        }


        //-----Burst Shot-----//
        private static IEnumerator FireBurstThree()
        {
            var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
            PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
            GameObject firePoint = grimmchild.FsmVariables.FindFsmGameObject("Flame Point").Value;
            var fireball = grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).gameObject.Value;
            yield return new WaitForSeconds(.25f);

            var fireball1 = GameObject.Instantiate(fireball);
            var fireball2 = GameObject.Instantiate(fireball);
            fireball1.transform.position = firePoint.transform.position;
            fireball2.transform.position = firePoint.transform.position;


            grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 0;
            var targetpos = grimmchild.GetState("Shoot").GetAction<FaceObject>(1).objectB.Value.transform.position;


            float storeAngle1;
            float num = targetpos.y + 0 - fireball1.transform.position.y;
            float num2 = targetpos.x + 0 - fireball1.transform.position.x;
            float num3 = Mathf.Atan2(num, num2) * 57.295776f;
            
            
            storeAngle1 = num3 - 30f;
            float valuex;
            float valuey;
            valuex = 30 * Mathf.Cos(storeAngle1 * 0.017453292f);
            valuey = 30 * Mathf.Sin(storeAngle1 * 0.017453292f);
            Vector2 velocity;
            velocity.x = valuex;
            velocity.y = valuey;
            fireball1.GetComponent<Rigidbody2D>().velocity = velocity;

            float storeAngle2;
            float num4 = targetpos.y + 0 - fireball1.transform.position.y;
            float num5 = targetpos.x + 0 - fireball1.transform.position.x;
            float num6 = Mathf.Atan2(num4, num5) * 57.295776f;
            
            storeAngle2 = num6 + 30f;
            float valuex2;
            float valuey2;
            valuex2 = 30 * Mathf.Cos(storeAngle2 * 0.017453292f);
            valuey2 = 30 * Mathf.Sin(storeAngle2 * 0.017453292f);
            Vector2 velocity2;
            velocity2.x = valuex2;
            velocity2.y = valuey2;
            fireball2.GetComponent<Rigidbody2D>().velocity = velocity2;
            
        }
        private static IEnumerator FireBurstFive()
        {
            var gc = HeroController.instance.transform.Find("Charm Effects").gameObject.LocateMyFSM("Spawn Grimmchild");
            PlayMakerFSM grimmchild = gc.FsmVariables.FindFsmGameObject("Child").Value.LocateMyFSM("Control");
            GameObject grimmchildobj = gc.FsmVariables.FindFsmGameObject("Child").Value; ;
            GameObject firePoint = grimmchild.FsmVariables.FindFsmGameObject("Flame Point").Value;
            var fireball = grimmchild.GetState("Shoot").GetAction<SpawnObjectFromGlobalPool>(4).gameObject.Value;
            yield return new WaitForSeconds(.25f);

            var fireball1 = GameObject.Instantiate(fireball);
            var fireball2 = GameObject.Instantiate(fireball);
            var fireball3 = GameObject.Instantiate(fireball);
            var fireball4 = GameObject.Instantiate(fireball);
            fireball1.transform.position = firePoint.transform.position;
            fireball2.transform.position = firePoint.transform.position;
            

            grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 0;
            var targetpos = grimmchild.GetState("Shoot").GetAction<FaceObject>(1).objectB.Value.transform.position;


            float storeAngle1;
            float num = targetpos.y + 0 - fireball1.transform.position.y;
            float num2 = targetpos.x + 0 - fireball1.transform.position.x;
            float num3 = Mathf.Atan2(num, num2) * 57.295776f;
            
            storeAngle1 = num3 - 30f;
            float valuex;
            float valuey;
            valuex = 30 * Mathf.Cos(storeAngle1 * 0.017453292f);
            valuey = 30 * Mathf.Sin(storeAngle1 * 0.017453292f);
            Vector2 velocity;
            velocity.x = valuex;
            velocity.y = valuey;
            fireball1.GetComponent<Rigidbody2D>().velocity = velocity;

            float storeAngle2;
            float num4 = targetpos.y + 0 - fireball1.transform.position.y;
            float num5 = targetpos.x + 0 - fireball1.transform.position.x;
            float num6 = Mathf.Atan2(num4, num5) * 57.295776f; 
           
            storeAngle2 = num6 + 30f;
            float valuex2;
            float valuey2;
            valuex2 = 30 * Mathf.Cos(storeAngle2 * 0.017453292f);
            valuey2 = 30 * Mathf.Sin(storeAngle2 * 0.017453292f);
            Vector2 velocity2;
            velocity2.x = valuex2;
            velocity2.y = valuey2;
            fireball2.GetComponent<Rigidbody2D>().velocity = velocity2;
            grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 15;



            yield return new WaitForSeconds(.2f);
            fireball3.transform.position = firePoint.transform.position;
            fireball4.transform.position = firePoint.transform.position;


            float storeAngle3;
            float num7 = targetpos.y + 0 - fireball3.transform.position.y;
            float num8 = targetpos.x + 0 - fireball3.transform.position.x;
            float num9 = Mathf.Atan2(num7, num8) * 57.295776f; 
            
            storeAngle3 = num9 - 15f;
            float valuex3;
            float valuey3;
            valuex3 = 30 * Mathf.Cos(storeAngle3 * 0.017453292f);
            valuey3 = 30 * Mathf.Sin(storeAngle3 * 0.017453292f);
            Vector2 velocity3;
            velocity3.x = valuex3;
            velocity3.y = valuey3;
            fireball3.GetComponent<Rigidbody2D>().velocity = velocity3;

            float storeAngle4;
            float num10 = targetpos.y + 0 - fireball4.transform.position.y;
            float num11 = targetpos.x + 0 - fireball4.transform.position.x;
            float num12 = Mathf.Atan2(num10, num11) * 57.295776f; 
            
            storeAngle4 = num12 + 15f;
            float valuex4;
            float valuey4;
            valuex4 = 30 * Mathf.Cos(storeAngle4 * 0.017453292f);
            valuey4 = 30 * Mathf.Sin(storeAngle4 * 0.017453292f);
            Vector2 velocity4;
            velocity4.x = valuex4;
            velocity4.y = valuey4;
            fireball4.GetComponent<Rigidbody2D>().velocity = velocity4;
            grimmchild.GetState("Shoot").GetAction<FireAtTarget>(7).spread = 15;
        }
    }
}