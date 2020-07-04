using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIRepeater : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)]
        public AI.AINodeConnection output;

        [Range(1, 10)]
        public int repeats = 1;

        [AISerializedVariable]
        [HideInInspector]
        public int currentRepeat;
        [AISerializedVariable]
        [HideInInspector]
        public bool hadFailure;
        public override void OnInit() {
            currentRepeat = 0;
            hadFailure = false;
        }
        public override AIStatus OnProcess() {
            while (currentRepeat < repeats) {
                AIBaseNode node = (GetOutputPort("output").Connection.node as AIBaseNode);
                AIStatus status = (graph as AIGraph).Process(node);
                switch (status) {
                    case AIStatus.Running:
                        return status;
                    case AIStatus.Success:
                        currentRepeat++;
                        continue;
                    case AIStatus.Failure:
                        currentRepeat++;
                        hadFailure = true;
                        continue;
                }
            }
            return hadFailure ? AIStatus.Failure : AIStatus.Success;
        }
        public override void OnInterrupt() {
        }
    }
}
