using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIConditions", menuName = "AI/Conditions", order = 0)]
public class AIScriptableConditions : ScriptableObject {
    public bool IsNight() {
        return false;
    }
}
