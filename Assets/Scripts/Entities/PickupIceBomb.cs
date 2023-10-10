using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupIceBomb : Pickup
{
    [SerializeField] private Sprite HUDIcon;
    [SerializeField] private Projectile.ProjectileType associatedProjectileType;




    public override bool Activate(Player user) {
        if (levelScript.SpawnProjectile(user, associatedProjectileType)) {
            //SetActive(false); //wot
            //levelScript.RegisterPickupDispawn(spawnPointIndex); //nonsense
            return true;
        }
        return false;
    }
    protected override void OnPickup(Player script) {
        script.RegisterPickup(this, HUDIcon);
        SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
