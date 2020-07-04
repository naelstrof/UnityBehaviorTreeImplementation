using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AISucceeder : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Tooltip("Returns \"Success\" no matter what. (Well, it still will return \"Running\" when it's running.")]
        [Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)]
        public AI.AINodeConnection output;

        public override void OnInit() {
        }
        public override AIStatus OnProcess() {
            AIBaseNode node = (GetOutputPort("output").Connection.node as AIBaseNode);
            AIStatus status = (graph as AIGraph).Process(node);
            switch(status) {
                case AIStatus.Failure:
                    return AIStatus.Success;
                case AIStatus.Success:
                    return AIStatus.Success;
                case AIStatus.Running:
                    return status;
                default:
                    return status;
            }
        }
        public override void OnInterrupt() {
        }
    }
}
