using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInstance;

public class PickupRocket : Pickup
{
    [SerializeField] private Sprite HUDIcon;
    [SerializeField] private Projectile.ProjectileType associatedProjectileType;



    public override void Initialize() {
        type = PickupType.ROCKET;
        initialized = true;
    }
    public override bool Activate(Player user) {
        if (levelScript.SpawnProjectile(user, associatedProjectileType)) {
            if (GetInstance().GetCurrentGameMode() == GameMode.LAN)
                GetInstance().GetRpcManagerScript().UpdateProjectileSpawnRequestServerRpc(GetInstance().GetClientID(), user.GetPlayerType(), associatedProjectileType);
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
