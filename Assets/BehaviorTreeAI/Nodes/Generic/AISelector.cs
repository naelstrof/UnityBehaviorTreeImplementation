using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AISelector : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Output(dynamicPortList = true, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never)]
        public List<AI.AINodeConnection> nodes;

        [AISerializedVariable]
        [NonSerialized]
        [HideInInspector]
        public int currentNodeIndex;
        public override void OnInit() {
            currentNodeIndex = 0;
        }
        public override AIStatus OnProcess() {
            while (currentNodeIndex < nodes.Count) {
                AIBaseNode node = (GetOutputPort("nodes " + currentNodeIndex).Connection.node as AIBaseNode);
                AIStatus status = (graph as AIGraph).Process(node);
                switch(status) {
                    case AIStatus.Failure:
                        currentNodeIndex++;
                        continue;
                    case AIStatus.Success:
                    case AIStatus.Running:
                        return status;
                }
            }
            return AIStatus.Failure;
        }
        public override void OnInterrupt() {

        }
    }
}
