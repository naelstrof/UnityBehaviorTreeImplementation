using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIFindNearby : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Range(1f,100f)]
        public float maxDistance;
        public LayerMask mask;
        public string tag;
        public override AIStatus OnProcess() {
            foreach (Collider c in Physics.OverlapSphere((graph as AIGraph).GetVariable<Transform>("transform").position, maxDistance, mask, QueryTriggerInteraction.Collide)) {
                if (tag == "" || c.CompareTag(tag)) {
                    (graph as AIGraph).SetVariable("targetObject", c.gameObject);
                    return AIStatus.Success;
                }
            }
            return AIStatus.Failure;
        }
    }
}
