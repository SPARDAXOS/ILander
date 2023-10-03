using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Initialization
{
    public class Intializer
    {

        [RuntimeInitializeOnLoadMethod]
        private static void InitGame()
        {


#if SHOW_APPLICATION_INFO
            Debug.Log("Started initializing game. Test out define symbols!");
#endif
            var go = new GameObject("GameInstance");
            var comp = go.AddComponent<GameInstance>();
            comp.Initialize();
        }
    }

}