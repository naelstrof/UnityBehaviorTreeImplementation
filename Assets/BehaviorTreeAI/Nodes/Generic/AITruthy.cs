using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {

    [Serializable]
    public class AICondition : SerializableCallback<bool> { }
    public class AITruthy : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        public List<AICondition> conditions;
        public override void OnInit() {
        }
        public override AIStatus OnProcess() {
            foreach( AICondition c in conditions) {
                if (!c.Invoke()) {
                    return AIStatus.Failure;
                }
            }
            return AIStatus.Success;
        }
        public override void OnInterrupt() {
        }
    }
}

