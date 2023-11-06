using UnityEngine;

public class KillBox : MonoBehaviour {




    private void OnTriggerEnter2D(Collider2D collision) {
        var script = collision.GetComponent<Player>();
        if (script)
            script.TakeDamage(999);
    }
}
