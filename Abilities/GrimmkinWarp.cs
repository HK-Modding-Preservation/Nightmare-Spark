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
        // public static GameObject activeTorch;
        public static bool choice = false;
        public static bool conditions = false;
        public static float oldscale = 1;
        internal static void SceneChange(Scene From, Scene To)
        {
            if (GameManager.instance.transform.Find("GlobalPool").Find("Warp Torch") != null)
            {
                pressCount = 0;
                warpActive = false;
                warped = false;
                choice = false;
                Time.timeScale = oldscale;
                var torch = GameManager.instance.transform.Find("GlobalPool").Find("Warp Torch").gameObject;
                torch.active = false;
                GameManager.instance.StopCoroutine(Warp());
            }
        }

        public static void WarpMain()
        {
            if (PlayerData.instance.GetBool($"equippedCharm_{Nightmare_Spark.Instance.CharmIDs[0]}") && PlayerDataAccess.equippedCharm_37)
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
                                if (noviceObj.Find("Explode Effects").Find("Smoke").GetComponent<ParticleSystem>().isPlaying)
                                {
                                    warpActive = false;
                                    pressCount = 0;
                                    Nightmare_Spark.Instance.Log("Didn't do shit");
                                    break;
                                }
                                else
                                {
                                    LaunchGrimmkin();
                                    GameManager.instance.StartCoroutine(SetActiveFalse(1));
                                    break;
                                }
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
            var cState = HeroControllerR.cState;
            if (!cState.dashing && !cState.attacking && !cState.dead && !cState.casting && !cState.focusing &&
                !cState.isPaused && !cState.swimming &&
                !HeroControllerR.controlReqlinquished && !HeroControllerR.hardLanded && PlayerDataAccess.hasSuperDash)
            {
                conditions = true;
            }
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
            var dashClip = grimmkinNovice.LocateMyFSM("Control").GetState("Dash").GetAction<Tk2dPlayAnimationWithEvents>(0).clipName.Value;
            noviceObj.GetComponent<MeshRenderer>().enabled = true;
            noviceObj.GetComponent<tk2dSpriteAnimator>().Play(dashClip);
            noviceObj.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = true;
            noviceObj.GetComponent<BoxCollider2D>().enabled = true;
            int ws;
            if (HeroController.instance.wallSlidingR || HeroController.instance.wallSlidingL) { ws = -1; } else { ws = 1; }
            if (HeroController.instance.wallSlidingR || HeroController.instance.wallSlidingL) { noviceObj.transform.position = HeroController.instance.transform.position + new Vector3(facing ? -.5f : .5f,0); }
            else { noviceObj.transform.position = HeroController.instance.transform.position + new Vector3(facing ? -.25f : .25f, 0); }
            noviceObj.GetComponent<Rigidbody2D>().transform.localScale = new Vector2(facing ? -1f *ws : 1f*ws, 1);

            noviceObj.GetComponent<Rigidbody2D>().velocity = new Vector2(facing ? 15 * ws : -15 * ws, 0);

            HeroController.instance.transform.Find("Torch Indicator").gameObject.active = true;
            HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Flame").gameObject.active = false;
            HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Pt Orbs").gameObject.active = false;
            
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
            noviceObj.Find("Explode Effects").active = true;
            noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().enabled = true;
            noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().maxParticleSize = .1f;
            noviceObj.Find("Explode Effects").Find("Smoke").GetComponent<ParticleSystemRenderer>().enabled = true;
            noviceObj.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = false;
            noviceObj.GetComponent<MeshRenderer>().enabled = false;
            noviceObj.GetComponent<BoxCollider2D>().enabled = false;
            var torch = GameManager.instance.transform.Find("GlobalPool").Find("Warp Torch");
            //activeTorch = torch.gameObject;
            torch.Find("Active Effects").gameObject.active = true;
            torch.transform.position = noviceObj.transform.position;
            torch.gameObject.active = true;
            GameManager.instance.StartCoroutine(Warp());
        }

        private static IEnumerator Warp()
        {
            var torch = GameManager.instance.transform.Find("GlobalPool").Find("Warp Torch").gameObject;
            yield return new WaitUntil(() => torch.active);
            HeroController.instance.cState.onGround = false;
            HeroController.instance.transform.position = torch.transform.position;
            GameManager.instance.cameraCtrl.SnapTo(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y);
            torch.active = false;         
            HeroController.instance.transform.Find("Torch Indicator").gameObject.active = false;
            oldscale = Time.timeScale;
            Time.timeScale = .1f;
            yield return new WaitForSeconds(.2f);
            Time.timeScale = oldscale;
            yield return new WaitUntil(() => !noviceObj.Find("Explode Effects").Find("Smoke").GetComponent<ParticleSystem>().isPlaying);
            noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().enabled = false;
            noviceObj.Find("Explode Effects").Find("Smoke").GetComponent<ParticleSystemRenderer>().enabled = false;
            noviceObj.Find("Explode Effects").active = false;
            warped = true;

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
                if (collision.gameObject.name == "Thorn Collider (6)")
                {
                    Nightmare_Spark.Instance.Log(collision.gameObject.layer);
                }
            }

            public IEnumerator SpawnTorch()
            {

                yield return new WaitForSeconds(.75f);
                HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Flame").gameObject.active = true;
                HeroController.instance.transform.Find("Torch Indicator").Find("Active Effects").Find("Pt Orbs").gameObject.active = true;
                noviceObj.Find("Explode Effects").active = true;
                noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().enabled = true;
                noviceObj.Find("Explode Effects").Find("grimm_flame_particle").GetComponent<ParticleSystemRenderer>().maxParticleSize = .1f;
                noviceObj.Find("Explode Effects").Find("Smoke").GetComponent<ParticleSystemRenderer>().enabled = true;
                noviceObj.Find("Pt Orbs").GetComponent<ParticleSystem>().enableEmission = false;
                noviceObj.GetComponent<MeshRenderer>().enabled = false;
                noviceObj.GetComponent<BoxCollider2D>().enabled = false;
                var torch = GameManager.instance.transform.Find("GlobalPool").Find("Warp Torch").gameObject;
                torch.Find("Active Effects").gameObject.active = true;
                torch.transform.position = gameObject.transform.position;
                torch.active = true;
                //activeTorch = torch.gameObject;
                hitWall = true;
            }
        }
    }
}
