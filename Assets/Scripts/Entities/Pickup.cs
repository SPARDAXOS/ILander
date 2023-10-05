using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    private PickupData data;

    private bool active = false;

    public void SetActive(bool state) {
        active = state;
    }
    public bool GetActive() {
        return active;
    }


    public void SetPickupData(PickupData data) {
        this.data = data;
    }


    public abstract void Activate(Player user);


    private void OnTriggerEnter2D(Collider2D collision) {
        var playerScript = collision.GetComponent<Player>();
        if (!playerScript)
            return;

        playerScript.RegisterPickup(this);
        SetActive(false);
        gameObject.SetActive(false);
    }
}
