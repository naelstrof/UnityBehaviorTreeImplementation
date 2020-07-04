using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using AI;

namespace AI {
    public class AIParallel : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Tooltip("Runs nodes in parallel, interrupts on failure, runs until all nodes returned success.")]
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        public List<AI.AINodeConnection> nodes;

        [AISerializedVariable]
        [HideInInspector]
        public int successMask;

        private void Abort() {
            for (int i = 0; i < nodes.Count; i++) {
                NodePort port = GetOutputPort("nodes " + i);
                if (port == null || !port.IsConnected) {
                    continue;
                }
                AIBaseNode node = (port.Connection.node as AIBaseNode);
                if (node == null) {
                    continue;
                }
                if (((1<<i) & successMask) == 1) {
                    continue;
                }
                (graph as AIGraph).Deinitialize(node);
            }
        }
        public override void OnInit() {
            successMask = 0;
        }
        public override AIStatus OnProcess() {
            for(int i=0;i<nodes.Count;i++) {
                NodePort port = GetOutputPort("nodes " + i);
                if (port == null || !port.IsConnected) {
                    continue;
                }
                AIBaseNode node = (port.Connection.node as AIBaseNode);
                if (node == null) {
                    continue;
                }
                if (((1<<i) & successMask) == 1) {
                    continue;
                }
                AIStatus status = (graph as AIGraph).Process(node);
                switch (status) {
                    case AIStatus.Failure:
                        Abort();
                        return status;
                    case AIStatus.Success:
                        successMask |= (1 << i);
                        continue;
                    case AIStatus.Running:
                        continue;
                }
            }
            // See if we're all done.
            for(int i=0;i<nodes.Count;i++) {
                if ((successMask & (1<<i)) == 0) {
                    return AIStatus.Running;
                }
            }
            return AIStatus.Success;
        }
        public override void OnInterrupt() {
        }
    }
}
