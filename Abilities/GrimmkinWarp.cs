using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Nightmare_Spark
{
    internal class GrimmkinWarp
    {
        public static int pressCount = 0;
        public static bool warpActive = false;
        public static bool warped = false;
        public static GameObject noviceObj;
        public static GameObject activeTorch;
        public static bool choice = false;
        public static bool conditions = false;
        internal static void SceneChange(Scene From, Scene To)
        {
            pressCount = 0;
            warpActive = false;
            warped = false; 
        }

        public static void WarpMain()
        {
            if (PlayerData.instance.GetBool($"equippedCharm_{Nightmare_Spark.Instance.CharmIDs[0]}") && PlayerData.instance.GetBool("equippedCharm_37"))
            {
                var heroActions = InputHandler.Instance.inputActions;

                if (heroActions.superDash.IsPressed && !warpActive)
                {
                    CheckConditions();
                    if (conditions)
                    {
                        warpActive = true;

                        pressCount++;
                        switch (pressCount)
                        {
                            case 1:
                                LaunchGrimmkin();
                                GameManager.instance.StartCoroutine(SetActiveFalse(1));
                                break;
                            case 2:
                                if (noviceObj.GetComponent<NoviceBehaviour>().hitWall && !choice)
                                {
                                    choice = true;
                                    GameManager.instance.StartCoroutine(Warp());
                                }
                                if (!noviceObj.GetComponent<NoviceBehaviour>().hitWall && !choice)
                                {
                                    choice = true;
                                    StopMovement();

                                }
                                GameManager.instance.StartCoroutine(SetActiveFalse(2));
                                break;
                        }
                    }
                }
            }
        }

        private static void CheckConditions()
        {
            var reflection = Satchel.Reflected.HeroControllerR.cState;
            if (!reflection.dashing && !reflection.attacking && !reflection.dead && !reflection.casting && !reflection.focusing && !reflection.isPaused && !reflection.swimming &&
                !HeroController.instance.controlReqlinquished && !Satchel.Reflected.HeroControllerR.hardLanded && PlayerData.instance.GetBool("hasSuperDash")){conditions = true;}
            else 
            {
                conditions = false;
            }
        }

        private static IEnumerator SetActiveFalse(int a)
        {
            switch (a)
            {
                case 1:
                    yield return new WaitForSeconds(.5f);
                    warpActive = false;
                    break;
                case 2:
                    yield return new WaitUntil(() => warped = true);
                    
                    yield return new WaitForSeconds(1);
                    warpActive = false;
                    pressCount = 0;
                    choice = false;
                    break;
            }
        }
        private static void LaunchGrimmkin()
        {
            warped = false;
            bool facing = HeroController.instance.cState.facingRight;
            var grimmkinNovice = Nightmare_Spark.grimmkinSpawnerSmall.LocateMyFSM("Spawn Control").GetState("Level 1").GetAction<CreateObject>(0).gameObject.Value;
            var novice = GameObject.Instantiate(grimmkinNovice);
            GameObject.Destroy(novice.LocateMyFSM("Control"));
            GameObject.Destroy(novice.GetComponent<DamageHero>());
            novice.RemoveComponent<HealthManager>();
            novice.RemoveComponent<EnemyDreamnailReaction>();
            novice.AddComponent<NonBouncer>();
            novice.AddComponent<NoviceBehaviour>();
            GameObject transitionCollider = new();
            novice.layer = (int)PhysLayers.DEFAULT;
            var dashClip = grimmkinNovice.LocateMyFSM("Control").GetState("Dash").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
            novice.GetComponent<tk2dSpriteAnimator>().Play(dashClip);
            novice.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = true;
            novice.GetComponent<BoxCollider2D>().enabled = true;
            int ws;
            if (HeroController.instance.wallSlidingR || HeroController.instance.wallSlidingL) { ws = -1; } else { ws = 1; }
            if (HeroController.instance.wallSlidingR || HeroController.instance.wallSlidingL) { novice.transform.position = HeroController.instance.transform.position + new Vector3(facing ? -.5f : .5f,0); }
            else { novice.transform.position = HeroController.instance.transform.position + new Vector3(facing ? -.25f : .25f, 0); }
            novice.GetComponent<Rigidbody2D>().transform.localScale = new Vector2(facing ? -1f *ws : 1f*ws, 1);

            novice.GetComponent<Rigidbody2D>().velocity = new Vector2(facing ? 15 * ws : -15 * ws, 0);

            HeroController.instance.transform.Find("Torch Indicator").gameObject.active = true;
            HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Flame").gameObject.active = false;
            HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Pt Orbs").gameObject.active = false;
            noviceObj = novice;
        }
        private static void StopMovement()
        {
            var clipDie = Nightmare_Spark.grimmkinSpawnerSmall.LocateMyFSM("Spawn Control").GetState("Level 1").GetAction<CreateObject>(0).gameObject.Value.LocateMyFSM("Control").GetState("Death Start").GetAction<Tk2dPlayAnimation>(4).clipName.Value;
            noviceObj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            noviceObj.Find("Pt Dying").GetComponent<ParticleSystem>().enableEmission = true;
            noviceObj.GetComponent<tk2dSpriteAnimator>().Play(clipDie);
            GameManager.instance.StartCoroutine(SpawnTorch());
        }


        private static IEnumerator SpawnTorch()
        {
            yield return new WaitForSeconds(.25f);
            noviceObj.Find("Explode Effects").SetActive(true);
            noviceObj.Find("Explode Effects").Find("Flame Ring").active = false;
            noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().maxParticleSize = .1f;
            noviceObj.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = false;
            Modding.Logger.Log("Finished die clip");
            noviceObj.GetComponent<MeshRenderer>().enabled = false;
            Modding.Logger.Log("Spawning Torch");
            var torch = GameObject.Instantiate(Nightmare_Spark.grimmkinSpawnerSmall);
            GameObject.Destroy(torch.LocateMyFSM("Spawn Control"));
            GameObject.Destroy(torch.Find("Hero Detector"));
            activeTorch = torch.gameObject;
            torch.transform.position = noviceObj.transform.position;
            torch.active = true;
            GameManager.instance.StartCoroutine(Warp());
        }

        private static IEnumerator Warp()
        {         
            yield return new WaitUntil(() => activeTorch != null);
            HeroController.instance.transform.position = activeTorch.transform.position;
            GameObject.Destroy(activeTorch);
            warped = true;
            HeroController.instance.transform.Find("Torch Indicator").gameObject.active = false;

        }

        public class NoviceBehaviour : MonoBehaviour
        {
            public bool hitWall = false;
            void OnTriggerEnter2D(Collider2D collision)
            {
                if (collision.gameObject.layer == (int)PhysLayers.TERRAIN || collision.gameObject.layer == (int)PhysLayers.TRANSITION_GATES || collision.gameObject.Find("EnemyDetector").tag == "Enemy Message")
                {
                    hitWall = true;
                    var clipDie = Nightmare_Spark.grimmkinSpawnerSmall.LocateMyFSM("Spawn Control").GetState("Level 1").GetAction<CreateObject>(0).gameObject.Value.LocateMyFSM("Control").GetState("Death Start").GetAction<Tk2dPlayAnimation>(4).clipName.Value;
                    gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    gameObject.Find("Pt Dying").GetComponent<ParticleSystem>().enableEmission = true;
                    gameObject.GetComponent<tk2dSpriteAnimator>().Play(clipDie);
                    GameManager.instance.StartCoroutine(SpawnTorch());
                    
                }
            }

            public IEnumerator SpawnTorch()
            {
                yield return new WaitForSeconds(.75f);
                HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Flame").gameObject.active = true;
                HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Pt Orbs").gameObject.active = true;
                gameObject.Find("Explode Effects").SetActive(true);
                noviceObj.Find("Explode Effects").Find("Flame Ring").active = false;
                noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().maxParticleSize = .1f;
                gameObject.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = false;
                Modding.Logger.Log("Finished die clip");
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                Modding.Logger.Log("Spawning Torch");
                var torch = GameObject.Instantiate(Nightmare_Spark.grimmkinSpawnerSmall);
                GameObject.Destroy(torch.LocateMyFSM("Spawn Control"));
                GameObject.Destroy(torch.Find("Hero Detector"));
                torch.transform.position = gameObject.transform.position;
                torch.active = true;
                activeTorch = torch.gameObject;
                hitWall = true;
            }
        }
    }
}
