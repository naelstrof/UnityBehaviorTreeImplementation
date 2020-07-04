using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIWait : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        public float waitTimeMin;
        public float waitTimeMax;

        [AISerializedVariable]
        [HideInInspector]
        [NonSerialized]
        public float waitTime;

        [AISerializedVariable]
        [HideInInspector]
        [NonSerialized]
        public float startedTime;
        public override void OnInit() {
            startedTime = Time.timeSinceLevelLoad;
            waitTime = UnityEngine.Random.Range(waitTimeMin, waitTimeMax);
        }
        public override AIStatus OnProcess() {
            if (Time.timeSinceLevelLoad > startedTime + waitTime) {
                return AIStatus.Success;
            }
            return AIStatus.Running;
        }
        public override void OnInterrupt() {
        }
    }
}
