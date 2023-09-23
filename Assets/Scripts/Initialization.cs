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
            var go = new GameObject("GameInstance");
            var comp = go.AddComponent<GameInstance>();
            comp.Initialize();
            comp.Initialize();

        }
    }

}