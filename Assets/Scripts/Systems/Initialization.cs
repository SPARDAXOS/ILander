using System.Collections;
using System.Collections.Generic;
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
                Debug.Log("Started initializing game. sada sa");
#endif
            var go = new GameObject("GameInstance");
            var comp = go.AddComponent<GameInstance>();
            comp.Initialize();
        }
    }

}