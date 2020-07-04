using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using AI;

namespace AI {
    public class AIParallelRepeater : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Tooltip("Runs nodes in parallel repeatedly, interrupts all on failure, runs until a node returned failure, or until every node has returned success at least once.")]
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        public List<AI.AINodeConnection> nodes;

        [AISerializedVariable]
        [HideInInspector]
        public List<int> successMask;

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
                (graph as AIGraph).Deinitialize(node);
            }
        }
        public override void OnInit() {
            if (successMask == null) {
                successMask = new List<int>();
            }
            for (int i = 0; i < nodes.Count; i++) {
                if (successMask.Count <= i) {
                    successMask.Add(0);
                    continue;
                }
                successMask[i] = 0;
            }
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
                AIStatus status = (graph as AIGraph).Process(node);
                switch (status) {
                    case AIStatus.Failure:
                        Abort();
                        return status;
                    case AIStatus.Success:
                        successMask[i]++;
                        continue;
                    case AIStatus.Running:
                        continue;
                }
            }
            // See if we're all done.
            for(int i=0;i<nodes.Count;i++) {
                if (successMask[i] == 0) {
                    return AIStatus.Running;
                }
            }
            return AIStatus.Success;
        }
        public override void OnInterrupt() {
        }
    }
}
