using UnityEngine;

public class Geometry : MonoBehaviour {
    [SerializeField] private float contactDamage = 0.03f;


    private void OnCollisionEnter2D(Collision2D collision) {
        var script = collision.gameObject.GetComponent<Player>();
        if (script)
            script.TakeDamage(contactDamage);
    }
}
