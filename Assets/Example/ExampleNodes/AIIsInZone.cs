using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIIsInZone : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;
        public override AIStatus OnProcess() {
            GameObject targetGameObject = (graph as AIGraph).GetVariable<GameObject>("targetObject");
            Collider c = targetGameObject.GetComponentInChildren<Collider>();
            Transform t = (graph as AIGraph).GetVariable<Transform>("transform");
            if (c.ClosestPoint(t.position) == t.position) {
                return AIStatus.Success;
            }
            return AIStatus.Failure;
        }
    }
}
