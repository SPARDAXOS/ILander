using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType {
        NONE = 0,
        ROCKET,
        ICE_BOMB
    }
    [SerializeField] protected ProjectileType type = ProjectileType.NONE;

    [SerializeField] private float damage = 0.1f;
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float spawnOffset = 10.0f;

    protected bool active = false;
    protected Player ownerScript = null;

    protected Vector2 currentDirection = Vector2.zero;

    public void SetActive(bool state) {
        active = state;
        gameObject.SetActive(state);

    }
    public bool IsActive() {
        return active;
    }
    public ProjectileType GetProjectileType() {
        return type;
    }
    private void ResetToStartState() {
        ownerScript = null;
        currentDirection = Vector2.zero;
        transform.position = Vector2.zero;
    }
    private void UpdateRotation() {

        transform.localRotation = Quaternion.LookRotation(transform.forward, currentDirection);
    }

    virtual public void Shoot(Player owner) {
        ownerScript = owner;
        currentDirection = owner.transform.up;
        Vector3 ownerPosition = owner.transform.position;
        transform.position = new Vector3(ownerPosition.x + (spawnOffset * currentDirection.x), ownerPosition.y + (spawnOffset * currentDirection.y), ownerPosition.z);
        SetActive(true);
        transform.localRotation = Quaternion.LookRotation(owner.transform.forward, owner.transform.up);
        transform.Rotate(0.0f, 0.0f, 90.0f); //Projectile sprite is facing right while player sprite is facing up
    }





    public void Tick() {




        UpdateMovement();
    }
    virtual protected void UpdateMovement() {
        Vector2 result = currentDirection * speed * Time.deltaTime;
        transform.position += new Vector3(result.x, result.y, 0.0f);
    }
    virtual protected void OnCollision(Collision2D collision) {

        Debug.Log("I HIT SOMEONE!");
        SetActive(false);
        ResetToStartState();
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        OnCollision(collision);
    }
}
