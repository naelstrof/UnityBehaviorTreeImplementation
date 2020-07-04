using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AICleanUp : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;
        public override AIStatus OnProcess() {
            GameObject g = (graph as AIGraph).GetVariable<GameObject>("targetObject");
            Animator a = (graph as AIGraph).GetVariable<Animator>("animator");
            if (a != null) {
                a.SetTrigger("PickUp");
            }
            g.SetActive(false);
            return AIStatus.Success;
        }
    }
}
