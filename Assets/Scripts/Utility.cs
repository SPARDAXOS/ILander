using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ILanderUtility {
    public class Utility
    {
        public static void Clamp(ref float target, float min, float max)
        {
            if (target > max)
                target = max;
            if (target < min)
                target = min;
        }
        public static bool Validate(object target, string errorMessage, bool abortOnFail = false)
        {
            if (target == null)
            {
                if (abortOnFail)
                    GameInstance.GetInstance().Abort(errorMessage);
                return false;
            }
            return true;
        }

    }
}
