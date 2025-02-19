﻿using UnityEngine;

namespace SG.RSC
{
    public class FireworkBoom : Firework
    {
        public float radius;

        public override void Boom()
        {
            if (activateFX != null)
            {
                GameObject fx = Instantiate(activateFX, t.position, t.rotation) as GameObject;
                fx.transform.SetParent(Game.ui.game.stuffBack, true);
                fx.layer = 11;
            }

            Bomb(radius);

            Destroy(gameObject);
        }
    }
}