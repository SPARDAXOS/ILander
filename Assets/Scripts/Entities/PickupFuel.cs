using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFuel : Pickup
{

    public override void Initialize() {
        type = PickupType.FUEL;
        initialized = true;
    }

    public override bool Activate(Player user) {
        user.AddFuel(Potency);
        return true;
    }
    protected override void OnPickup(Player script) {
        Activate(script);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
