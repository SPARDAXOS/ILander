using Unity.Netcode;
using UnityEngine;
using static GameInstance;

public class Projectile : MonoBehaviour
{
    //Primarily to make it easier to select associated projectiles in pickups entries.
    public enum ProjectileType {
        NONE = 0,
        ROCKET,
        ICE_BOMB
    }

    [SerializeField] protected float damage = 0.1f;
    [SerializeField] protected float speed = 10.0f;
    [SerializeField] protected Vector2 spawnOffset = new Vector2(0.5f, 0.5f);

    
    protected ProjectileType type = ProjectileType.NONE;
    protected bool active = false;
    protected bool initialized = false;
    protected Player ownerScript = null;

    protected bool moving = false;

    protected Vector2 currentDirection = Vector2.zero;


    protected BoxCollider2D boxCollider2DComp;
    protected SpriteRenderer spriteRendererComp;
    protected NetworkObject networkObjectComp;


    virtual public void SetActive(bool state) {
        active = state;
        gameObject.SetActive(state);
    }
    public bool IsActive() {
        return active;
    }
    public ProjectileType GetProjectileType() {
        return type;
    }


    virtual public void Initialize() {
        type = ProjectileType.NONE;
        networkObjectComp = GetComponent<NetworkObject>();
        spriteRendererComp = GetComponent<SpriteRenderer>();
        boxCollider2DComp = GetComponent<BoxCollider2D>();
        boxCollider2DComp.enabled = false;
        initialized = true;
    }
    virtual protected void ResetToStartState() {
        ownerScript = null;
        currentDirection = Vector2.zero;
        transform.position = Vector2.zero;
        moving = false;
        boxCollider2DComp.enabled = false;
    }

    virtual public bool Shoot(Player owner) {
        //Pos
        currentDirection = owner.transform.up;
        Vector3 ownerPosition = owner.GetMuzzleFlashPosition();
        transform.position = new Vector3(ownerPosition.x + (spawnOffset.x * currentDirection.x), ownerPosition.y + (spawnOffset.y * currentDirection.y), ownerPosition.z);
        //Rot
        transform.localRotation = Quaternion.LookRotation(owner.transform.forward, owner.transform.up);
        transform.Rotate(0.0f, 0.0f, 90.0f); //Projectile sprite is facing right while player sprite is facing up

        ownerScript = owner;
        SetActive(true);
        moving = true;
        boxCollider2DComp.enabled = true;
        return true;
    }
    virtual public void Dispawn() {
        SetActive(false);
        ResetToStartState();

    }




    virtual public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick uninitialized entity " + gameObject.name);
            return;
        }


        if (moving)
            UpdateMovement();
    }
    virtual protected void UpdateMovement() {
        Vector2 result = currentDirection * speed * Time.deltaTime;
        transform.position += new Vector3(result.x, result.y, 0.0f);
    }
    virtual protected void OnCollision(Collider2D collision) {
        Player script = collision.gameObject.GetComponent<Player>();
        if (script == ownerScript)
            return;

        Dispawn();
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        OnCollision(collision);
    }
}
