using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using AI;
using System;

namespace AI {
    public class AISequencer : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        [SerializeField]
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
                NodePort port = GetOutputPort("nodes " + currentNodeIndex);
                if (port == null || !port.IsConnected) {
                    currentNodeIndex++;
                    continue;
                }
                AIBaseNode node = (port.Connection.node as AIBaseNode);
                if (node == null) {
                    currentNodeIndex++;
                    continue;
                }
                AIStatus status = (graph as AIGraph).Process(node);
                switch (status) {
                    case AIStatus.Failure:
                        return status;
                    case AIStatus.Success:
                        currentNodeIndex++;
                        continue;
                    case AIStatus.Running:
                        return status;
                }
            }
            return AIStatus.Success;
        }
        public override void OnInterrupt() {

        }
    }
}
