using UnityEngine;

public class PickupFuel : Pickup {
    [SerializeField] private float Potency = 0.0f;

    public override void Initialize() {
        type = PickupType.FUEL;
        initialized = true;
    }
    public override bool Activate(Player user) {
        user.AddFuel(Potency);
        GameInstance.GetGameInstance().GetSoundManagerScript().PlaySFX("Effect", true, gameObject);
        return true;
    }
    protected override void OnPickup(Player script) {
        Activate(script);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
