using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupHealth : Pickup
{
    public override void Initialize() {
        type = PickupType.HEALTH;
        initialized = true;
    }
    public override bool Activate(Player user) {
        user.AddHealth(Potency);
        return true;
    }
    protected override void OnPickup(Player script) {
        Activate(script);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex); //GOT INDEX OUT OF BOUNDS HERE IN ONLINE TESTING!
    }
}
