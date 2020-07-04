using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIFindWanderPoint : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        public float wanderDistance;

        [AISerializedVariable]
        [HideInInspector]
        [NonSerialized]
        public Vector3 targetPoint;

        [AISerializedVariable]
        [HideInInspector]
        [NonSerialized]
        public bool foundPoint;
        bool RandomPoint(Vector3 center, float range, out Vector3 result) {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }
        public override void OnInit() {
            //direction = UnityEngine.Random.insideUnitSphere.GroundVector().normalized;
            Transform t = (graph as AIGraph).GetVariable<Transform>("transform");
            foundPoint = RandomPoint(t.position, wanderDistance, out targetPoint);
        }
        public override AIStatus OnProcess() {
            if (!foundPoint) {
                return AIStatus.Failure;
            }
            (graph as AIGraph).SetVariable("targetObject", targetPoint);
            return AIStatus.Success;
        }
        public override void OnInterrupt() {
        }
    }
}
