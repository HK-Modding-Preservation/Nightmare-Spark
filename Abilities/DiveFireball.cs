using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Spark
{
    internal class DiveFireball
    {
        public static void DiveFireballs(int damage, int spread)
        {
            int x = spread;
            for (int i = -x; i <= x; i += 12)
            {
                var fireball = GameObject.Instantiate(Nightmare_Spark.nkg.LocateMyFSM("Control").GetState("UP Explode").GetAction<SpawnObjectFromGlobalPool>(10).gameObject.Value);
                fireball.RemoveComponent<DamageHero>();
                Nightmare_Spark.AddDamageEnemy(fireball).damageDealt = damage;
                fireball.layer = (int)PhysLayers.HERO_ATTACK;
                GameObject.DontDestroyOnLoad(fireball);
                var rb2d = fireball.GetComponent<Rigidbody2D>();
                rb2d.velocity = new Vector2(i, 1);
                fireball.transform.position = HeroController.instance.transform.position - new Vector3(0, 0, 0);
            }


        }
    }
}
