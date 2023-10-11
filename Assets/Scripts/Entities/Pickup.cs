using UnityEngine;

public abstract class Pickup : MonoBehaviour {
    //Primarily for book keeping purposes and association with projectiles.
    public enum PickupType {
        NONE = 0,
        HEALTH,
        FUEL,
        ROCKET,
        ICE_BOMB
    }

    protected bool active = false;
    protected bool initialized = false;
    protected PickupType type = PickupType.NONE;

    protected Level levelScript;
    protected int spawnPointIndex = -1;
    protected int ID = -1;

    public void SetActive(bool state) {
        active = state;
        gameObject.SetActive(state);
    }
    public bool IsActive() {
        return active;
    }
    public void SetPickupID(int id) {
        ID = id;
    }
    public int GetPickupID() {
        return ID;
    }
    public void SetSpawnPointIndex(int index) {
        spawnPointIndex = index;
    }
    public int GetSpawnPointIndex() {
        return spawnPointIndex;
    }
    public PickupType GetPickupType() {
        return type;
    }
    public void SetLevelScript(Level script) {
        levelScript = script;
    }


    virtual public void Initialize() {
        type = PickupType.NONE;
        initialized = true;
    }
    public abstract bool Activate(Player user);
    protected virtual void OnPickup(Player script) {
        script.RegisterPickup(this);
        SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        var playerScript = collision.GetComponent<Player>();
        if (!playerScript)
            return;

        OnPickup(playerScript);
    }
}
