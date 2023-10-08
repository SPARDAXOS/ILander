using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupHealth : Pickup
{
    public override bool Activate(Player user) {
        user.AddHealth(Potency);
        return true;
    }
    protected override void OnPickup(Player script) {
        Activate(script);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
