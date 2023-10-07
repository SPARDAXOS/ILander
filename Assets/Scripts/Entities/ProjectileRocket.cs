using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRocket : Projectile
{


    protected override void OnCollision(Collision2D collision) {
        //base.OnCollision(collision);
        Debug.Log("I hit " + collision.gameObject.name);
        //damage player for amount
        //Dispawn by setting active to false
        //Play explosion somehow
        //Should add dispawner after time

    }

}
