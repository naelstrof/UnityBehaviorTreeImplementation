using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class KoboldCharacterController : MonoBehaviour {
    [System.Serializable]
    public class PID {
        public float P;
        //public float I;
        public float D;
        private float error=0f;
        private float errorDifference = 0f;
        private float lastError=0f;
        //private float rollingError = 0f;
        private float timeStep = 0f;

        public PID(float initP, float initI, float initD) {
            P = initP;
            //I = initI;
            D = initD;
        }
        public void UpdatePID(float newError, float timeElapsed) {
            error = newError;
            errorDifference = error - lastError;
            //rollingError = Mathf.Lerp(rollingError, error, 0.1f);
            lastError = error;
            timeStep = timeElapsed;
        }
        public float GetCorrection() {
            return error * P * timeStep * 100f + errorDifference * D * timeStep * 200f;
            //return error * P * timeStep * 100f + rollingError * I + errorDifference * D * timeStep * 200f;
        }

    }
    private Rigidbody body;
    private Vector3 velocity = new Vector3(0, 0, 0);
    private float effectiveSpeed {
        get {
            float s = Mathf.Lerp(grounded?speed:speed*airSpeedMultiplier, crouchSpeed, crouchAmount);
            return Mathf.Lerp(s, s*walkSpeedMultiplier, (grounded && inputWalking) ? 1f : 0f);
        }
    }
    private float effectiveAccel {
        get {
            float a = Mathf.Lerp(accel, crouchAccel, crouchAmount);
            return grounded ? a : airAccel;
        }
    }
    private float effectiveStepHeight {
        get {
            return Mathf.Lerp(stepHeight, stepHeightCrouched, crouchAmount);
        }
    }
    // Only check below the player's feet when we're continually grounded.
    private float effectiveStepCheckDistance {
        get {
            return (jumpTimer > 0 || !grounded) ? effectiveStepHeight : stepHeightCheckDistance;
        }
    }
    private float effectiveFriction {
        get {
            return Mathf.Lerp(friction, crouchFriction, Mathf.Round(crouchAmount));
        }
    }

    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    // True for the frame that impulse was applied for a jump.
    public bool jumped = false;
    [HideInInspector]
    // This is to keep the player from instantly snapping back to the floor, a timer for when to activate the ground PID again.
    public float jumpTimer = 0f;
    [HideInInspector]
    // How far the player has crouched, analog
    public float crouchAmount = 0;
    // Used to keep track of objects the player is standing on.
    [HideInInspector]
    public Vector3 groundVelocity;
    [HideInInspector]
    public Rigidbody groundRigidbody;

    // ----- These variables are all for just making sounds -----
    // How far the ground is from the player's origin, literally only used for spawning sounds.
    private float distanceToGround = 0;
    // Used to check if the player has hit the ground so we can play a sound.
    private bool groundedMemory = false;
    private float stepWidth = 140f;
    private float stepCounter = 0;
    //public List<AudioClip> footsteps = new List<AudioClip>();
    //public List<AudioClip> footlands = new List<AudioClip>();
    // ---------------------------------------------------------

    // These are just used to cache the collider's original data, since we modify them to crouch.
    private float colliderFullHeight;
    private Vector3 colliderNormalCenter;

    public float airSpeedMultiplier = 1.2f;
    // Inputs, controlled by external player or AI script
    public Vector3 inputDir = new Vector3(0, 0, 0);
    public bool inputJump = false;
    public bool inputCrouched = false;
    public bool inputWalking = false;

    [Tooltip("Gravity applied per second to the character, generally to make the gravity feel less floaty.")]
    public Vector3 gravityMod = new Vector3(0, -0.25f, 0);
    [Tooltip("Fixed impulse for how high the character jumps.")]
    public float jumpStrength = 8f;
    [Tooltip("The speed at which acceleration is no longer applied when the player is moving.")]
    public float speed = 19f;
    [Tooltip("The speed at which acceleration is no longer applied when the player is crouch-moving.")]
    public float crouchSpeed = 13f;
    [Tooltip("Speed multiplier for when inputWalking is true.")]
    public float walkSpeedMultiplier = 0.4f;
    [Tooltip("How quickly the player reaches max speed while walking.")]
    public float accel = 5f;
    [Tooltip("How quickly the player reaches max speed while crouch-walking.")]
    public float crouchAccel = 2f;
    [Tooltip("How quickly the player reaches max speed while in the air.")]
    public float airAccel = 0.3f;
    [SerializeField]
    [Tooltip("How high the character should float off the ground. (measured from capsule center to ground)")]
    public float stepHeight = 1.2f;
    [Tooltip("How high the character should float off the ground while crouched. (measured from capsule center to ground)")]
    public float stepHeightCrouched = 0.6f;
    [Tooltip("How far to raycast in order to suck the player to the floor, necessary for walking down slopes. (measured from capsule center to ground)")]
    public float stepHeightCheckDistance = 1.6f;
    [Tooltip("Basically the constraint settings for keeping the player a certain distance from the floor.")]
    public PID groundingPID = new PID(0.9f,0f,1.8f);
    [Tooltip("How fat the raycast is to check for walkable ground under the capsule collider.")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("How sharp the player movement is, high friction means sharper movement.")]
    public float friction = 9f;
    [Tooltip("How sharp the player movement is while crouched, high friction means sharper movement.")]
    public float crouchFriction = 11f;
    [Tooltip("Mask of layers containing walkable ground. Should match up with whatever the capsule collides with.")]
    public LayerMask hitLayer;
    [Tooltip("The collider of the player capsule.")]
    new public CapsuleCollider collider;
    [Tooltip("How tall the collider should be during a full crouch.")]
    public float crouchHeight = 0.75f;
    [Tooltip("Reference to the player model so we can push it up and down based on when we crouch.")]
    public Transform worldModel;

    private void Start() {
        body = GetComponent<Rigidbody>();
        colliderFullHeight = collider.height;
        colliderNormalCenter = collider.center;
    }

    // Check if we've clipped our capsule into something.
    private bool Stuck() {
        Vector3 topPoint = collider.transform.TransformPoint(collider.center + new Vector3(0, collider.height / 2, 0));
        Vector3 botPoint = collider.transform.TransformPoint(collider.center);
        foreach(Collider c in Physics.OverlapCapsule(topPoint, botPoint, collider.radius/2, hitLayer, QueryTriggerInteraction.Ignore) ) {
            return true;
        }
        return false;
    }
    private void CheckCrouched() {
        // Calculate height difference of just the collider
        float oldHeight = collider.height;
        collider.height = Mathf.MoveTowards(collider.height, inputCrouched ? crouchHeight : colliderFullHeight, Time.deltaTime*2f);
        float diff = (collider.height - oldHeight) / 2f;
        // If we're uncrouching and we hit something, undo the crouch progress.
        if (diff > 0 && Stuck()) {
            collider.height = oldHeight;
        } else {
            float oldStepHeight = effectiveStepHeight;
            crouchAmount = collider.height.Remap(crouchHeight, colliderFullHeight, 1, 0);
            collider.center = Vector3.Lerp(colliderNormalCenter, colliderNormalCenter + new Vector3(0, (colliderFullHeight - crouchHeight) * 0.5f, 0f), crouchAmount);

            diff += (effectiveStepHeight - oldStepHeight);
            worldModel.position -= new Vector3(0, diff, 0);

            if (grounded) {
                body.MovePosition(body.position + new Vector3(0, diff, 0)/2f);
            } else {
                body.MovePosition(body.position - new Vector3(0, diff, 0)/2f);
            }
        }
    }

    private void Friction() {
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        if ( speed < 0.01f ) {
            velocity.x = 0;
            velocity.z = 0;
            return;
        }
        float stopSpeed = 1f;
        float control = speed < stopSpeed ? stopSpeed : speed;
        float drop = 0;
        if (grounded) {
            drop += control * effectiveFriction * Time.fixedDeltaTime;
        }
        float newspeed = speed - drop;
        if (newspeed < 0) {
            newspeed = 0;
        }
        newspeed /= speed;
        velocity.x *= newspeed;
        velocity.z *= newspeed;
    }

    private void GroundCalculate() {
        groundVelocity = Vector3.zero;
        RaycastHit hit;
        distanceToGround = 100f;
        bool actuallyHit = false;
        Vector3 floorNormal = Vector3.up;
        // Do a simple raycast before doing an array of 9 raycasts looking for walkable ground. This is to get accurate normal data, as a normal spherecast deflects the normal incorrectly at the edges.
        if (Physics.SphereCast(transform.position + colliderNormalCenter, groundCheckRadius, -transform.up, out hit, 5f, hitLayer)) {
            for(float x = -groundCheckRadius; x <= groundCheckRadius; x+=groundCheckRadius) {
                for (float y = -groundCheckRadius; y <= groundCheckRadius; y += groundCheckRadius) {
                    if (Physics.Raycast(transform.position + colliderNormalCenter + new Vector3(x,0,y), -transform.up, out hit, 5f, hitLayer)) {
                        floorNormal = hit.normal;
                        distanceToGround = hit.distance;
                        if (hit.normal.y >= 0.7f && hit.distance <= effectiveStepCheckDistance + 0.05f) {
                            actuallyHit = true;
                            break;
                        }
                    }
                }
                if (floorNormal.y >= 0.7f && distanceToGround <= effectiveStepCheckDistance + 0.05f) {
                    actuallyHit = true;
                    break;
                }
            }
        }
        if (hit.rigidbody) {
            groundVelocity = hit.rigidbody.GetPointVelocity(hit.point);
            groundRigidbody = hit.rigidbody;
        } else if (actuallyHit) {
            groundRigidbody = null;
        }
        if (distanceToGround <= effectiveStepCheckDistance + 0.05f && floorNormal.y >= 0.7f) {
            grounded = true;
            groundingPID.UpdatePID(-(distanceToGround - effectiveStepHeight), Time.fixedDeltaTime);
            if (jumpTimer <= 0) {
                velocity += Vector3.up * groundingPID.GetCorrection();
            } else {
                // Don't push the capsule down if we're jumping.
                velocity += Vector3.up * Mathf.Max(groundingPID.GetCorrection(), 0f);
            }
        } else {
            floorNormal = Vector3.up;
            grounded = false;
        }
    }

    private void AirBrake(Vector3 wishdir) {
        float d = Vector3.Dot(Vector3.Normalize(wishdir), Vector3.Normalize(velocity));
        if ( d < 0 ) {
            velocity = Vector3.ProjectOnPlane(velocity, wishdir);
        }
    }

    private void Update() {
        if (!enabled) {
            return;
        }
        CheckCrouched();
    }

    private void CheckSounds() {
        /*if (grounded) {
            if ( grounded != groundedMemory ) {
                GameManager.instance.SpawnAudioClipInWorld(footlands[Random.Range(0, footlands.Count - 1)], transform.position + Vector3.down * distanceToGround, 0.25f);
            }
        }
        if (grounded) {
            stepCounter += velocity.GroundVector().magnitude;
            if (stepCounter > stepWidth) {
                GameManager.instance.SpawnAudioClipInWorld(footsteps[Random.Range(0, footsteps.Count - 1)], transform.position + Vector3.down * distanceToGround, 0.15f);
                stepCounter = 0;
            }
        }
        groundedMemory = grounded;*/
    }

    private void FixedUpdate() {
        if (!enabled) {
            return;
        }
        velocity = body.velocity;
        jumped = false;
        velocity -= groundVelocity;
        groundVelocity = Vector3.zero;
        GroundCalculate();
        if ( !grounded ) {
            velocity += gravityMod;
        }
        JumpCheck();
        AirBrake(inputDir);
        Friction();
        Accelerate(inputDir, effectiveSpeed, effectiveAccel);
        CheckSounds();
        velocity += groundVelocity;
        body.velocity = velocity;
    }

    private void JumpCheck() {
        jumpTimer -= Time.fixedDeltaTime;
        if ( grounded && inputJump ) {
            //if (jumpTimer < 0f) {
                //GameManager.instance.SpawnAudioClipInWorld(footsteps[Random.Range(0, footsteps.Count - 1)], transform.position + Vector3.down * distanceToGround, 0.15f);
            //}
            jumped = true;
            velocity.y = Mathf.Max(velocity.y, jumpStrength);
            grounded = false;
            jumpTimer = 0.25f;
        }
    }

    // Quake style acceleration
    void Accelerate(Vector3 wishdir, float wishspeed, float accel) {
        float addspeed, accelspeed, currentspeed;
        currentspeed = Vector3.Dot(velocity.GroundVector(), wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0) {
            return;
        }
        accelspeed = accel * Time.fixedDeltaTime * wishspeed;
        if (accelspeed > addspeed) {
            accelspeed = addspeed;
        }
        velocity += accelspeed * wishdir;
    }
}
