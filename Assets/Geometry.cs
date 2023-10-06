using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry : MonoBehaviour
{








    private void OnCollisionEnter2D(Collision2D collision) {
        var script = collision.gameObject.GetComponent<Player>();
        if (script) {

            Debug.Log("HIT!");


        }
    }
}
