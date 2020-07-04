using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AI {
    public class AIGoTo : AIBaseNode {
        [Input(backingValue = ShowBackingValue.Never)]
        public AI.AINodeConnection input;

        public float giveUpTime;
        //private float recalculateTime = 4f;

        [HideInInspector]
        [AISerializedVariable]
        public UnityEngine.AI.NavMeshPath path;

        [HideInInspector]
        [AISerializedVariable]
        public int pathNode;

        [HideInInspector]
        [AISerializedVariable]
        public bool foundPath = false;

        [HideInInspector]
        [AISerializedVariable]
        public float giveUpTimer;

        [HideInInspector]
        [AISerializedVariable]
        public float recalculateTimer;

        public override void OnInit() {
            //recalculateTimer = 4f;
            pathNode = 0;
            if (path == null) {
                path = new UnityEngine.AI.NavMeshPath();
            }
            Vector3 desiredPos;
            GameObject targetGameObject = (graph as AIGraph).GetVariable<GameObject>("targetObject");
            if (targetGameObject == null) {
                Vector3 targetPosition = (graph as AIGraph).GetVariable<Vector3>("targetObject");
                if (targetPosition == null) {
                    foundPath = false;
                    return;
                }
                desiredPos = targetPosition;
            } else {
                desiredPos = targetGameObject.transform.position;
            }
            UnityEngine.AI.NavMesh.CalculatePath((graph as AIGraph).GetVariable<Transform>("transform").position, desiredPos, UnityEngine.AI.NavMesh.AllAreas, path);
            foundPath = true;
            (graph as AIGraph).SetVariable("UpdateDelay", 0.1f);
            giveUpTimer = Time.timeSinceLevelLoad + giveUpTime;
            //recalculateTimer = Time.timeSinceLevelLoad + recalculateTime;
        }
        public override void OnInterrupt() {
            KoboldCharacterController controller = (graph as AIGraph).GetVariable<KoboldCharacterController>("controller");
            controller.inputDir = Vector3.zero;
            controller.inputJump = false;
        }
        public override AIStatus OnProcess() {
            //if (Time.timeSinceLevelLoad > recalculateTimer) {
                //float giveUpTimerSave = giveUpTimer;
                //OnInit();
                //giveUpTimer = giveUpTimerSave;
            //}
            if (foundPath == false) {
                return AIStatus.Failure;
            }
            //if (pathNode >= path.corners.Length) {
                //return AIStatus.Failure;
            //}
            Transform t = (graph as AIGraph).GetVariable<Transform>("transform");
            Vector3 pos = t.position;
            Vector3 desiredPos = Vector3.zero;
            if (pathNode < path.corners.Length) {
                desiredPos = path.corners[pathNode];
            } else {
                GameObject targetGameObject = (graph as AIGraph).GetVariable<GameObject>("targetObject");
                if (targetGameObject == null) {
                    Vector3 targetPosition = (graph as AIGraph).GetVariable<Vector3>("targetObject");
                    if (targetPosition == null) {
                        return AIStatus.Failure;
                    }
                    desiredPos = targetPosition;
                } else {
                    desiredPos = targetGameObject.transform.position;
                }
            }
            Vector3 dir = (desiredPos - pos).GroundVector().normalized;
            Rigidbody r = (graph as AIGraph).GetVariable<Rigidbody>("rigidbody");
            Vector3 axis;
            float angle;
            Quaternion.FromToRotation(t.forward, dir).ToAngleAxis(out angle, out axis);
            //r.AddTorque(Vector3.Project(r.angularVelocity, axis)*(graph as AIGraph).GetVariable<float>("UpdateDelay"));
            r.AddTorque(axis * angle * (graph as AIGraph).GetVariable<float>("UpdateDelay") * r.mass * 10f);

            KoboldCharacterController controller = (graph as AIGraph).GetVariable<KoboldCharacterController>("controller");
            controller.inputDir = dir;
            controller.inputJump = (desiredPos.y - pos.y) > 1f;
            Debug.DrawLine(t.position, desiredPos, Color.blue, (graph as AIGraph).GetVariable<float>("UpdateDelay"));

            if (Vector3.Distance(pos, desiredPos) < 1f) {
                pathNode++;
                giveUpTimer = Time.timeSinceLevelLoad + giveUpTime;
                //recalculateTimer = Time.timeSinceLevelLoad + recalculateTime;
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
    }
}
