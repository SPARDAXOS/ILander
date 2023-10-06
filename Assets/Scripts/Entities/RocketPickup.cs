using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPickup : Pickup
{
    [SerializeField] private Sprite HUDIcon;

    //GetRefToRocket? idk how im gonna spawn the thing. 


    public override void Activate(Player user) {
        

    }
    protected override void OnPickup(Player script) {
        script.RegisterPickup(this, HUDIcon);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }
}
