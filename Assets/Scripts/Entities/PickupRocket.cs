using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRocket : Pickup
{
    [SerializeField] private Sprite HUDIcon;
    [SerializeField] private Projectile.ProjectileType associatedProjectileType;


    public override void Activate(Player user) {
        levelScript.SpawnProjectile(user, associatedProjectileType);
        SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
    protected override void OnPickup(Player script) {
        script.RegisterPickup(this, HUDIcon);
        SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
