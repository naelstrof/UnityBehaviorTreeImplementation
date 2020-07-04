using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIEnterZone : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        public float giveUpTime;
        private float recalculateTime = 1f;

        [HideInInspector]
        [AISerializedVariable]
        public UnityEngine.AI.NavMeshPath path;

        [HideInInspector]
        [AISerializedVariable]
        public int pathNode;

        [HideInInspector]
        [AISerializedVariable]
        public float giveUpTimer;

        [HideInInspector]
        [AISerializedVariable]
        public float recalculateTimer;

        [HideInInspector]
        [AISerializedVariable]
        public bool alreadyInside;

        public override void OnInit() {
            GameObject targetGameObject = (graph as AIGraph).GetVariable<GameObject>("targetObject");
            Collider c = targetGameObject.GetComponentInChildren<Collider>();
            Transform t = (graph as AIGraph).GetVariable<Transform>("transform");
            if (c.ClosestPoint(t.position) == t.position) {
                alreadyInside = true;
                return;
            }
            pathNode = 0;
            if (path == null) {
                path = new UnityEngine.AI.NavMeshPath();
            }
            UnityEngine.AI.NavMesh.CalculatePath(t.position, targetGameObject.transform.position, UnityEngine.AI.NavMesh.AllAreas, path);
            (graph as AIGraph).SetVariable("UpdateDelay", 0.1f);
            giveUpTimer = Time.timeSinceLevelLoad + giveUpTime;
            recalculateTimer = Time.timeSinceLevelLoad + recalculateTime;
        }
        public override AIStatus OnProcess() {
            if (Time.timeSinceLevelLoad > recalculateTimer) {
                float giveUpTimerSave = giveUpTimer;
                OnInit();
                giveUpTimer = giveUpTimerSave;
            }
            if (alreadyInside) {
                return AIStatus.Failure;
            }
            GameObject targetGameObject = (graph as AIGraph).GetVariable<GameObject>("targetObject");
            Transform t = (graph as AIGraph).GetVariable<Transform>("transform");
            Vector3 pos = t.position;
            Vector3 desiredPos = Vector3.zero;
            if (pathNode < path.corners.Length) {
                desiredPos = path.corners[pathNode];
            } else {
                desiredPos = targetGameObject.transform.position;
            }
            Vector3 dir = (desiredPos - pos).GroundVector().normalized;
            Rigidbody r = (graph as AIGraph).GetVariable<Rigidbody>("rigidbody");
            Vector3 axis;
            float angle;
            Quaternion.FromToRotation(t.forward, dir).ToAngleAxis(out angle, out axis);
            r.AddTorque(Vector3.Project(r.angularVelocity, axis)*(graph as AIGraph).GetVariable<float>("UpdateDelay"));
            r.AddTorque(axis * angle * (graph as AIGraph).GetVariable<float>("UpdateDelay") * r.mass * 10f);

            KoboldCharacterController controller = (graph as AIGraph).GetVariable<KoboldCharacterController>("controller");
            controller.inputDir = dir;
            controller.inputJump = (desiredPos.y - pos.y) > 1f;

            // We made it inside!
            Collider c = targetGameObject.GetComponentInChildren<Collider>();
            if (c.ClosestPoint(pos) == pos) {
                return AIStatus.Success;
            }
            if (Vector3.Distance(pos, desiredPos) < 1f) {
                pathNode++;
                giveUpTimer = Time.timeSinceLevelLoad + giveUpTime;
                if (pathNode >= path.corners.Length) {
                    controller.inputDir = Vector3.zero;
                    controller.inputJump = false;
                    return AIStatus.Success;
                }
            }
            if (Time.timeSinceLevelLoad > giveUpTimer) {
                controller.inputDir = Vector3.zero;
                controller.inputJump = false;
                return AIStatus.Failure;
            }
            return AIStatus.Running;
        }
        public override void OnInterrupt() {
            KoboldCharacterController controller = (graph as AIGraph).GetVariable<KoboldCharacterController>("controller");
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
        }
    }
}
