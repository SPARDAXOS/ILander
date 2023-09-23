using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialization : MonoBehaviour
{
    


    private GameState gameState = null;



    void Start()
    {
        
    }
    void Update()
    {
        
    }




    private void Awake()
    {
        //INSTANCIATE game object prefab
        //Call init on gamestate script

        //Initializes all systems
    }


    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {

    }
}
