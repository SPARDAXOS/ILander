using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] protected float Potency = 0.0f;
    protected PickupEntryData entryData;
    protected bool active = false;

    public void SetActive(bool state) {
        active = state;
        gameObject.SetActive(state);
    }
    public bool GetActive() {
        return active;
    }


    public void SetPickupData(PickupEntryData data) {
        this.entryData = data;
    }
    public PickupEntryData GetData() { 
        return entryData;
    }


    public abstract void Activate(Player user);
    protected virtual void OnPickup(Player script) {
        script.RegisterPickup(this);
        SetActive(false);
        gameObject.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        var playerScript = collision.GetComponent<Player>();
        if (!playerScript)
            return;

        OnPickup(playerScript);
    }
}
