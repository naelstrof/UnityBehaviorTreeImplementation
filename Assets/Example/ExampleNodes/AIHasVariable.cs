using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using System.CodeDom;

namespace AI {
    public class AIHasVariable : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;
        public string variableName;
        public override AIStatus OnProcess() {
            object o = (graph as AIGraph).GetVariable<object>(variableName);
            if (o != null) {
                if (o.GetType() == typeof(int)) {
                    if ((int)o == 0) {
                        return AIStatus.Failure;
                    }
                    return AIStatus.Success;
                }
                if (o.GetType() == typeof(bool)) {
                    return (bool)o ? AIStatus.Success : AIStatus.Failure;
                }
            }
            return AIStatus.Failure;
        }
    }
}
