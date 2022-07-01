using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Spark
{
    internal class CarefreeSpikes
    {
        private static GameObject grimmSpike = Nightmare_Spark.grimmSpike;
        private static GameObject nightmareSpike = Nightmare_Spark.nightmareSpike;
        public class NightmareSpikeMonoBehaviour : MonoBehaviour
        {
           
            void Start()
            {
                GameManager.instance.StartCoroutine(SpikeAnimations());

            }
            IEnumerator SpikeAnimations()
            {
                var spikecollider = gameObject.GetComponent<PolygonCollider2D>();
                var clipready = nightmareSpike.LocateMyFSM("Control").GetState("Ready").GetAction<Tk2dPlayAnimation>(1).clipName.Value;
                var clipantic = nightmareSpike.LocateMyFSM("Control").GetState("Antic").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
                var clipup = nightmareSpike.LocateMyFSM("Control").GetState("Up").GetAction<Tk2dPlayAnimation>(0).clipName.Value;
                var clipdown = nightmareSpike.LocateMyFSM("Control").GetState("Down").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipready);
                yield return new WaitForSeconds(2.0015f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipantic);
                yield return new WaitForSeconds(0.2013f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipup);
                spikecollider.enabled = true;
                yield return new WaitForSeconds(0.7052f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipdown);
                spikecollider.enabled = false;
                yield return new WaitForSeconds(0.2018f);
                Destroy(gameObject);
            }
        }
        public class GrimmSpikeMonoBehaviour : MonoBehaviour
        {
            void Start()
            {
                GameManager.instance.StartCoroutine(SpikeAnimations());

            }
            IEnumerator SpikeAnimations()
            {
                var spikecollider = gameObject.GetComponent<PolygonCollider2D>();
                spikecollider.enabled = false;
                var clipready = grimmSpike.LocateMyFSM("Control").GetState("Ready").GetAction<Tk2dPlayAnimation>(1).clipName.Value;
                var clipantic = grimmSpike.LocateMyFSM("Control").GetState("Antic").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
                var clipup = grimmSpike.LocateMyFSM("Control").GetState("Up").GetAction<Tk2dPlayAnimation>(0).clipName.Value;
                var clipdown = grimmSpike.LocateMyFSM("Control").GetState("Down").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipready);
                yield return new WaitForSeconds(2.0015f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipantic);
                yield return new WaitForSeconds(0.2013f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipup);
                spikecollider.enabled = true;
                yield return new WaitForSeconds(0.7052f);
                gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipdown);
                spikecollider.enabled = false;
                yield return new WaitForSeconds(0.2018f);
                Destroy(gameObject);
            }
        }

        static bool active = false;
        public static void NightmareSpikeActivate()
        {
            GameObject carefreeshield = HeroController.instance.carefreeShield;

            if (carefreeshield.activeSelf == true && !active)
            {
               
                active = true;
                for (float i = 0; i <= 360; i += 30)
                {
                    GameObject spike = GameObject.Instantiate(nightmareSpike);
                    GameObject.Destroy(spike.LocateMyFSM("Control"));
                    GameObject.DontDestroyOnLoad(spike);
                    spike.SetActive(true);
                    spike.active = true;
                    spike.AddComponent<NightmareSpikeMonoBehaviour>();
                    spike.transform.position = HeroController.instance.transform.position;
                    spike.transform.SetRotationZ(i);
                    spike.layer = (int)PhysLayers.HERO_ATTACK;
                    GameObject.Destroy(spike.GetComponent<DamageHero>());
                    GameObject.Destroy(spike.GetComponent<TinkEffect>());
                    Nightmare_Spark.AddDamageEnemy(spike).damageDealt = 30;
                    spike.AddComponent<NonBouncer>();
                }
                GameManager.instance.StartCoroutine(SpikeWait());
            }
        }

        public static void GrimmSpikeActivate()
        {
            GameObject carefreeshield = HeroController.instance.carefreeShield;

            if (carefreeshield.activeSelf == true && !active)
            {

                active = true;
                for (float i = 0; i <= 360; i += 45)
                {
                    GameObject spike = GameObject.Instantiate(grimmSpike);
                    GameObject.Destroy(spike.LocateMyFSM("Control"));
                    GameObject.DontDestroyOnLoad(spike);
                    spike.GetComponent<MeshRenderer>().enabled = true;
                    spike.SetActive(true);
                    spike.active = true;
                    spike.AddComponent<GrimmSpikeMonoBehaviour>();
                    spike.transform.position = HeroController.instance.transform.position;
                    spike.transform.SetRotationZ(i);
                    spike.layer = (int)PhysLayers.HERO_ATTACK;
                    GameObject.Destroy(spike.GetComponent<DamageHero>());
                    GameObject.Destroy(spike.GetComponent<TinkEffect>());
                    Nightmare_Spark.AddDamageEnemy(spike).damageDealt = 20;
                    spike.AddComponent<NonBouncer>();
                }
                GameManager.instance.StartCoroutine(SpikeWait());
                
            }
        }

        private static IEnumerator SpikeWait()
        {
            
            yield return new WaitUntil(() => !HeroController.instance.carefreeShield.activeSelf);

            active = false;

               
            

        }
    }
}
