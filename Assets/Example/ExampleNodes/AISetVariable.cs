using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AISetVariable : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        [Output(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection output;

        public enum VariableType {
            String,
            Int, 
            Float,
            UnityObject
        }

        public string variableName;
        public VariableType variableType;
        public UnityEngine.Object uVariable;
        public string sVariable;
        public int iVariable;
        public float fVariable;


        [HideInInspector]
        [AISerializedVariable]
        public bool hasInitialized = false;

        public override void OnInit() {
            hasInitialized = false;
        }
        public override AIStatus OnProcess() {
            NodePort p = GetOutputPort("output");
            AIStatus status = AIStatus.Success;
            if (p.IsConnected) {
                if (!hasInitialized) {
                    (p.Connection.node as AIBaseNode).OnInit();
                    hasInitialized = true;
                }
                status = (graph as AIGraph).Process(p.Connection.node as AIBaseNode);
            }
            switch (status) {
                case AIStatus.Success:
                case AIStatus.Failure:
                    hasInitialized = false;
                    switch(variableType) {
                        case VariableType.String: (graph as AIGraph).SetVariable(variableName, sVariable); break;
                        case VariableType.Float: (graph as AIGraph).SetVariable(variableName, fVariable); break;
                        case VariableType.UnityObject: (graph as AIGraph).SetVariable(variableName, uVariable); break;
                        case VariableType.Int: (graph as AIGraph).SetVariable(variableName, iVariable); break;
                    }
                    return status;
                case AIStatus.Running:
                    return status;
            }
            return status;
        }
    }
}
