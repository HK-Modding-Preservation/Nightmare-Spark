using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Spark
{

    internal class ShapeOfGrimm
    {
        private static GameObject Bat;
        public static int GrimmSlugVelocity;
        public static int GrimmSlugIndicatorRange;
        private static Vector3 tether;
        private static string batEndClip;
        private static GameObject[] link = new GameObject[6];
        public static bool cancelGs;

        //private static float tetherx;
        //private static float tethery;
        public static bool gsActive = false;


        public static void GrimmSlugMovement()
        {
            var sc = HeroController.instance.spellControl;
            if (gsActive)
            {
                int gsvertical;
                int gshorizontal;
                var heroActions = InputHandler.Instance.inputActions;

                if (heroActions.up.IsPressed)
                {
                    gsvertical = GrimmSlugVelocity;
                }
                else
                {
                    if (heroActions.down.IsPressed)
                    {
                        gsvertical = -GrimmSlugVelocity;
                    }
                    else
                    {
                        gsvertical = 0;
                    }
                }
                if (heroActions.right.IsPressed)
                {
                    gshorizontal = GrimmSlugVelocity;
                    Bat.transform.localScale = new Vector3(1, 1, 1);

                }
                else
                {
                    if (heroActions.left.IsPressed)
                    {
                        gshorizontal = -GrimmSlugVelocity;
                        Bat.transform.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        gshorizontal = 0;
                    }
                }

                sc.GetState("Focus S").GetAction<SetParticleEmissionRate>(9).emissionRate.Value = 0f;
                sc.GetState("Focus S").GetAction<SetParticleEmissionRate>(10).emissionRate.Value = 0f;
                sc.GetState("Focus Left").GetAction<SetParticleEmissionRate>(9).emissionRate.Value = 0f;
                sc.GetState("Focus Left").GetAction<SetParticleEmissionRate>(10).emissionRate.Value = 0f;
                sc.GetState("Focus Right").GetAction<SetParticleEmissionRate>(9).emissionRate.Value = 0f;
                sc.GetState("Focus Right").GetAction<SetParticleEmissionRate>(10).emissionRate.Value = 0f;
                HeroController.instance.GetComponent<Rigidbody2D>().velocity = new Vector2(gshorizontal, gsvertical);

                float distance = HeroController.instance.transform.position.sqrMagnitude - tether.sqrMagnitude;
                float xdistance = HeroController.instance.transform.position.x - tether.x;
                float ydistance = (HeroController.instance.transform.position.y - 1) - tether.y + 1;
                for (int i = 0; i < 5; i++)
                {
                    if (link[i] != null)
                    {
                        link[i].transform.position = tether - new Vector3(-(xdistance / 5) * i, -(ydistance / 5 * i) + 1, 0);
                    }


                }
            }

            if (Bat != null)
            {
                Bat.transform.position = HeroController.instance.transform.position - new Vector3(0, 1, 0);
            }

            if (gsActive && !sc.GetState("Focus Cancel 2").GetAction<SetBoolValue>(16).boolVariable.Value || gsActive && !sc.GetState("Focus Get Finish 2").GetAction<SetBoolValue>(15).boolVariable.Value || gsActive && cancelGs)// || gsActive && HeroController.instance.controlReqlinquished)
            {
                gsActive = false;
               
               

                sc.AddTransition("Focus S", "LEFT GROUND", "Grace Check 2");
                sc.AddTransition("Focus Left", "LEFT GROUND", "Grace Check 2");
                sc.AddTransition("Focus Right", "LEFT GROUND", "Grace Check 2");

                HeroController.instance.AffectedByGravity(true);

                HeroController.instance.transform.Find("Focus Effects").Find("Lines Anim").GetComponent<tk2dSprite>().color = new Color(1f, 1, 1, 1);

                Bat.GetComponent<tk2dSpriteAnimator>().Play(batEndClip);
                Bat.GetComponent<tk2dSpriteAnimator>().AnimationCompleted += (caller, clip) =>
                {
                    if (clip.name == batEndClip)
                    {
                        GameObject.Destroy(Bat);
                        foreach (GameObject obj in link)
                        {
                            GameObject.Destroy(obj);
                        }

                        HeroController.instance.gameObject.GetComponent<MeshRenderer>().enabled = true;
                    }
                };
                if (cancelGs)
                {
                    HeroController.instance.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }

            }

        }

        public static void GrimmSlug()
        {
            var sc = HeroController.instance.spellControl;

            HeroController.instance.AffectedByGravity(false);

            sc.RemoveTransition("Focus S", "LEFT GROUND");
            sc.RemoveTransition("Focus Left", "LEFT GROUND");
            sc.RemoveTransition("Focus Right", "LEFT GROUND");

            Bat = GameObject.Instantiate(Nightmare_Spark.realBat);
            Bat.layer = (int)PhysLayers.PLAYER;
            Bat.SetActive(true);
            batEndClip = Bat.LocateMyFSM("Control").GetState("End").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
            GameObject.Destroy(Bat.LocateMyFSM("Control"));
            Bat.RemoveComponent<HealthManager>();
            Bat.GetComponent<MeshRenderer>().enabled = true;



            HeroController.instance.transform.Find("Focus Effects").Find("Lines Anim").GetComponent<tk2dSprite>().color = new Color(0.7f, 0, 0, 1);
           


            HeroController.instance.gameObject.GetComponent<MeshRenderer>().enabled = false;

            if (PlayerDataAccess.equippedCharm_7)
            {
                if (PlayerDataAccess.equippedCharm_34)
                {
                    // Quick Focus + Deep Focus 
                    GrimmSlugVelocity = 12;
                    GrimmSlugIndicatorRange = 7;
                }
                else
                {
                    // Quick Focus
                    GrimmSlugVelocity = 18;
                    GrimmSlugIndicatorRange = 5;
                }
            }
            else
            {
                if (PlayerDataAccess.equippedCharm_34)
                {
                    // Deep Focus
                    GrimmSlugVelocity = 9;
                    GrimmSlugIndicatorRange = 8;
                }
                else
                {
                    // Base
                    GrimmSlugVelocity = 15;
                    GrimmSlugIndicatorRange = 6;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                link[i] = GameObject.Instantiate(Nightmare_Spark.nkg.LocateMyFSM("Control").GetState("AD Fire").GetAction<SpawnObjectFromGlobalPoolOverTime>(7).gameObject.Value);
                link[i].transform.localScale = new Vector3(.1f, .1f, 0);
                link[i].GetComponent<ParticleSystem>().startSize = .01f;
                link[i].GetComponent<ParticleSystemRenderer>().maxParticleSize = .01f;
                link[i].GetComponent<ParticleSystem>().loop = true;
                link[i].LocateMyFSM("Control").RemoveTransition("State 2", "FINISHED");
                UnityEngine.Object.Destroy(link[i].LocateMyFSM("damages_hero"));
                link[i].AddComponent<NonBouncer>();
            }

            gsActive = true;

            tether = HeroController.instance.transform.position;
            GameObject circle = new();
            circle.AddComponent<Circle>();
            circle.transform.position = HeroController.instance.transform.position - new Vector3(0, 1.1f, 0);
            /*GameObject collider = new();
            collider.transform.parent = circle.transform;
            collider.name = "Collider";
            collider.AddComponent<CircleCollider2D>();
            collider.GetComponent<CircleCollider2D>().radius = GrimmSlugIndicatorRange;
            collider.GetComponent<CircleCollider2D>().transform.position = tether;
            //collider.GetComponent<CircleCollider2D>()
            collider.layer = (int)PhysLayers.HERO_DETECTOR;*/
            
            GameManager.instance.StartCoroutine(Timer(circle));
            
            

        }
        private static IEnumerator Timer(GameObject circle)
        {
            yield return new WaitUntil(() => !HeroController.instance.spellControl.FsmVariables.FindFsmBool("Focusing").Value || cancelGs);
            GameObject.Destroy(circle);


        }
        [RequireComponent(typeof(LineRenderer))]
        public class Circle : MonoBehaviour
        {
            [Range(0, 55)]
            public int segments = 70;
            [Range(0, 5)]
            public float xradius = GrimmSlugIndicatorRange;
            [Range(0, 5)]
            public float yradius = GrimmSlugIndicatorRange;
            LineRenderer limit;

            public Color startColor = Color.red;
            public Color endColor = Color.red;
            private void Start()
            {
                limit = gameObject.GetComponent<LineRenderer>();
                limit.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                limit.name = "Limit";
                limit.startWidth = .1f;
                limit.endWidth = .1f;
                limit.SetVertexCount(segments + 1);
                limit.useWorldSpace = false;
                CreatePoints();
                float alpha = 1.0f;
                Gradient gradient = new();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(startColor, 1.0f), new GradientColorKey(endColor, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 1.0f) }
                );
                limit.colorGradient = gradient;

            }
            void CreatePoints()
            {
                float x;
                float y;
                //float z;

                float angle = 20f;

                for (int i = 0; i < (segments + 1); i++)
                {
                    x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
                    y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

                    limit.SetPosition(i, new Vector3(x, y, 0));

                    angle += (360f / segments);
                }
            }
            private Vector3 previousheropos;
            public void LateUpdate()
            {
                var HCpos = HeroController.instance.transform.position;

                var diff = new Vector2(HCpos.x - tether.x, HCpos.y - tether.y);
                if (diff.sqrMagnitude > GrimmSlugIndicatorRange*GrimmSlugIndicatorRange)
                {
                    HeroController.instance.transform.position = previousheropos;
                }
                else
                {
                    previousheropos = HCpos;
                }

            }
          
        }
    }
}
