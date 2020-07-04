using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIInverter : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

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
                    return AIStatus.Failure;
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
