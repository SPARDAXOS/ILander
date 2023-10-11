using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ILanderUtility {
    public class Utility
    {
        public enum ValidationLevel {
            DEBUG,
            WARNING,
            ERROR
        }


        public static void Clamp(ref float target, float min, float max) {
            if (target > max)
                target = max;
            if (target < min)
                target = min;
        }
        public static bool Validate(object target, string message, ValidationLevel level, bool abortOnFail = false) {
            if (target == null) {
                if (level == ValidationLevel.DEBUG)
                    Debug.Log(message);
                else if (level == ValidationLevel.WARNING)
                    Debug.LogWarning(message);
                else if (level == ValidationLevel.ERROR)
                    Debug.LogError(message);

                if (abortOnFail)
                    GameInstance.GetGameInstance().Abort(message);
                return false;
            }
            return true;
        }

    }
}
