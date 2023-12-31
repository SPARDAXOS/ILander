using UnityEngine;

public class ProjectileIceBomb : Projectile {
    [SerializeField] private float freezeDuration = 1.0f;
    private Animator animatorComp;
    

    private void DefaultHitReaction() {
        animatorComp.SetTrigger("Hit"); //Will call dispawn at the end of anim
        moving = false;
        boxCollider2DComp.enabled = false;
    }
    private void PlayerHitReaction(Player script) {
        script.TakeDamage(damage);
        script.ApplyFreeze(freezeDuration);
        animatorComp.SetTrigger("Hit");
        moving = false;
        boxCollider2DComp.enabled = false;
    }

    public override void Initialize() {
        base.Initialize();

        type = ProjectileType.ICE_BOMB;
        animatorComp = GetComponent<Animator>();
    }
    public override bool Shoot(Player owner) {
        ownerScript = owner;
        return ownerScript.PlayMuzzleFlashAnim("ProjectileIceBomb", DelayedShoot, gameObject.transform.localScale);
    }
    public void DelayedShoot() {
        base.Shoot(ownerScript);
    }

    protected override void OnCollision(Collider2D collision) {
        var tag = collision.tag;
        if (tag == "Pickup")
            return;

        if (tag == "Player") {
            Player script = collision.gameObject.GetComponent<Player>();
            if (script == ownerScript)
                return;
            else
                PlayerHitReaction(script);
        }
        else
            DefaultHitReaction();
    }
}
