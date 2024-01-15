using UnityEngine;
using static GameInstance;

public class PickupRocket : Pickup {
    [SerializeField] private Sprite HUDIcon;
    [SerializeField] private Projectile.ProjectileType associatedProjectileType;


    public override void Initialize() {
        type = PickupType.ROCKET;
        initialized = true;
    }
    public override bool Activate(Player user) {
        if (levelScript.SpawnProjectile(user, associatedProjectileType)) {
            if (GetGameInstance().GetCurrentGameMode() == GameMode.LAN) {
                var instance = GetGameInstance();
                var rpcManager = instance.GetRpcManagerScript();
                rpcManager.UpdateProjectileSpawnRequestServerRpc(instance.GetClientID(), user.GetPlayerType(), associatedProjectileType);
            }
            GetGameInstance().GetSoundManagerScript().PlaySFX("Shoot", true, gameObject);
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
