using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelPickup : Pickup
{


    public override void Activate(Player user) {
        user.AddFuel(Potency);
    }
    protected override void OnPickup(Player script) {

        Activate(script);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
