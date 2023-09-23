using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameState : MonoBehaviour
{

    private static GameState instance;
    private GameObject instanceGameObject;


    void Start()
    {
        
    }
    void Update()
    {
        
    }



    //public static GameState GetInstance()
    //{
    //    //if (!instance)
    //    //{
    //    //    instance = new GameState();
    //    //    return instance;
    //    //}
        
    //    //Destroy(gameObject);
    //}



    public void Initialize()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //Instance?  
            return;
        }

        Destroy(gameObject);
    }
}
