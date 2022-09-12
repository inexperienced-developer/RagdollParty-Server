using InexperiencedDeveloper.Extensions;
using InexperiencedDeveloper.Utils.Log;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class LegMuscles
    {
        private readonly Player player;
        private readonly Ragdoll ragdoll;
        private readonly RagdollMovement movement;
        private float ballRadius;
        private PhysicMaterial ballMat;
        private PhysicMaterial footMat;
        private float ballFriction;
        private float footFriction;

        private float upImpulse;
        private float forwardImpulse;
        private int framesToApplyJumpImpulse;

        public LegMuscles(Player player, Ragdoll ragdoll, RagdollMovement movement)
        {
            this.player = player;
            this.ragdoll = ragdoll;
            this.movement = movement;
            ballRadius = (ragdoll.Ball.Collider as SphereCollider).radius;
            ballMat = ragdoll.Ball.Collider.material;
            footMat = ragdoll.RightFoot.Collider.material;
            ballFriction = ballMat.staticFriction;
            footFriction = footMat.staticFriction;
        }

        public void OnFixedUpdate(Vector3 torsoFeedback)
        {
            switch (player.State)
            {
                case PlayerState.Idle:
                    //DebugLogger.LogWarning($"TODO: Check additional movement parameters");
                    IdleAnimation(torsoFeedback, 1f);
                    break;
                case PlayerState.Run:
                    RunAnimation(torsoFeedback, 1f);
                    break;
                case PlayerState.Jump:
                    JumpAnimation(torsoFeedback);
                    break;
            }
        }

        #region Animation Variables
        public float LegPhase;
        #endregion

        private void IdleAnimation(Vector3 torsoFeedback, float rigidity)
        {
            RagdollMovement.AlignToVector(ragdoll.LeftThigh, -ragdoll.LeftThigh.Transform.up, Vector3.up, 10f * rigidity);
            RagdollMovement.AlignToVector(ragdoll.LeftLeg, -ragdoll.LeftLeg.Transform.up, Vector3.up, 10f * rigidity);
            RagdollMovement.AlignToVector(ragdoll.RightThigh, -ragdoll.RightThigh.Transform.up, Vector3.up, 10f * rigidity);
            RagdollMovement.AlignToVector(ragdoll.RightLeg, -ragdoll.RightLeg.Transform.up, Vector3.up, 10f * rigidity);
            ragdoll.Ball.Rigidbody.SafeAddForce(torsoFeedback * 0.2f, ForceMode.Force);
            ragdoll.LeftFoot.Rigidbody.SafeAddForce(torsoFeedback * 0.4f, ForceMode.Force);
            ragdoll.RightFoot.Rigidbody.SafeAddForce(torsoFeedback * 0.4f, ForceMode.Force);
            ragdoll.RightFoot.Rigidbody.angularVelocity = Vector3.zero;
        }

        private void RunAnimation(Vector3 torsoFeedback, float rigidity)
        {
            LegPhase = Time.realtimeSinceStartup * 1.5f;
            torsoFeedback += AnimateLeg(ragdoll.LeftThigh, ragdoll.LeftLeg, ragdoll.LeftFoot, LegPhase, torsoFeedback, rigidity);
            torsoFeedback += AnimateLeg(ragdoll.RightThigh, ragdoll.RightLeg, ragdoll.RightFoot, LegPhase + 0.5f, torsoFeedback, rigidity);
            ragdoll.Ball.Rigidbody.SafeAddForce(torsoFeedback, ForceMode.Force);
            RotateBall();
            AddWalkForce();
        }

        private void JumpAnimation(Vector3 torsoFeedback)
        {
            ragdoll.Hips.Rigidbody.SafeAddForce(torsoFeedback, ForceMode.Force);
            if (player.Jump)
            {
                float gravityMod = 2f;
                int num2 = 2;
                float jumpForce = Mathf.Sqrt(2f * gravityMod / Physics.gravity.magnitude);
                float groundSpeed = Mathf.Clamp(player.GroundManager.GroundSpeed.y, 0f, 100f);
                groundSpeed = Mathf.Pow(groundSpeed, 1.2f);
                jumpForce += (groundSpeed / Physics.gravity.magnitude);
                float adjJumpForce = jumpForce * player.Weight;
                float movementForce = player.Controls.UnsmoothedWalkSpeed * ((float)num2 + groundSpeed / 2f) * player.Mass;
                Vector3 momentum = player.Momentum;
                momentum.y = 1;
                Debug.Log($"Ground speed: {player.GroundManager.GroundSpeed.y}");
                Debug.Log($"Weight: {player.Weight}");
                float forwardMod = Vector3.Dot(player.Controls.WalkDir.normalized, momentum);
                if (forwardMod < 0f)
                {
                    forwardMod = 0f;
                }
                upImpulse = adjJumpForce * momentum.y;
                if (upImpulse < 0f)
                    upImpulse = 0f;
                forwardImpulse = movementForce - forwardMod;
                if (forwardImpulse < 0f)
                {
                    forwardImpulse = 0f;
                }
                framesToApplyJumpImpulse = 1;
                if (player.Grounded || Time.time - player.GetComponent<Ball>().TimeSinceLastNonZeroImpulse > 0.2f)
                {
                    upImpulse /= framesToApplyJumpImpulse;
                    forwardImpulse /= framesToApplyJumpImpulse;
                    ApplyJumpImpulses();
                    framesToApplyJumpImpulse--;
                }
            }
            else
            {
                if (framesToApplyJumpImpulse > 1)
                {
                    ApplyJumpImpulses();
                }
                int inputMomentumMultiplier = 3;
                int chestLeadLimit = 500;
                float inputMomentum = player.Controls.UnsmoothedWalkSpeed * (float)inputMomentumMultiplier * player.Mass;
                Vector3 momentum2 = player.Momentum;
                float momentumInputDot = Vector3.Dot(player.Controls.WalkDir.normalized, momentum2);
                float adjInputMomentum = inputMomentum - momentumInputDot;
                float force = Mathf.Clamp(adjInputMomentum, 0f, (float)chestLeadLimit);
                ragdoll.Chest.Rigidbody.SafeAddForce(force * player.Controls.WalkDir.normalized, ForceMode.Force);
            }
        }

        private void ApplyJumpImpulses()
        {
            float jumpMod = 1f;
            //If grabbing someone lower jump impulse
            for (int i = 0; i < player.GroundManager.GroundObjects.Count; i++)
            {
                //Add check to see if holding on to something on the ground
            }
            Vector3 adjUpImpulse = Vector3.up * upImpulse * jumpMod;
            Vector3 adjForwardImpulse = player.Controls.WalkDir * forwardImpulse * jumpMod;
            ragdoll.Head.Rigidbody.SafeAddForce(adjUpImpulse * 0.1f + adjForwardImpulse * 0.1f, ForceMode.Impulse);
            ragdoll.Chest.Rigidbody.SafeAddForce(adjUpImpulse * 0.1f + adjForwardImpulse * 0.1f, ForceMode.Impulse);
            ragdoll.Waist.Rigidbody.SafeAddForce(adjUpImpulse * 0.1f + adjForwardImpulse * 0.1f, ForceMode.Impulse);
            ragdoll.Hips.Rigidbody.SafeAddForce(adjUpImpulse * 0.1f + adjForwardImpulse * 0.1f, ForceMode.Impulse);
            ragdoll.Ball.Rigidbody.SafeAddForce(adjUpImpulse * 0.1f + adjForwardImpulse * 0.1f, ForceMode.Impulse);
            ragdoll.LeftThigh.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.LeftLeg.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.LeftFoot.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.RightThigh.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.RightLeg.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.RightFoot.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.LeftArm.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.LeftForearm.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.RightArm.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            ragdoll.RightForearm.Rigidbody.SafeAddForce(adjUpImpulse * 0.05f + adjForwardImpulse * 0.05f, ForceMode.Impulse);
            player.GroundManager.DistributeForce(-adjUpImpulse / Time.fixedDeltaTime, ragdoll.Ball.Rigidbody.position);
        }

        private Vector3 AnimateLeg(BodySegment thigh, BodySegment leg, BodySegment foot, float phase, Vector3 torsoFeedback, float rigidity)
        {
            rigidity *= 1f;
            phase -= Mathf.Floor(phase);
            if (phase < 0.2f)
            {
                RagdollMovement.AlignToVector(thigh, thigh.Transform.up, player.Controls.WalkDir + Vector3.down, 3f * rigidity);
                RagdollMovement.AlignToVector(leg, thigh.Transform.up, -player.Controls.WalkDir - Vector3.up, rigidity);
                Vector3 force = Vector3.up * 20f;
                foot.Rigidbody.SafeAddForce(force, ForceMode.Force);
                return -force;
            }
            if (phase < 0.5f)
            {
                RagdollMovement.AlignToVector(thigh, thigh.Transform.up, player.Controls.WalkDir, 2f * rigidity);
                RagdollMovement.AlignToVector(leg, thigh.Transform.up, player.Controls.WalkDir, 3f * rigidity);
            }
            else
            {
                if (phase < 0.7f)
                {
                    Vector3 force = torsoFeedback * 0.2f;
                    foot.Rigidbody.SafeAddForce(force, ForceMode.Force);
                    RagdollMovement.AlignToVector(thigh, thigh.Transform.up, player.Controls.WalkDir + Vector3.down, rigidity);
                    RagdollMovement.AlignToVector(leg, thigh.Transform.up, Vector3.down, rigidity);
                    return -force;
                }
                if (phase < 0.9f)
                {
                    Vector3 force = torsoFeedback * 0.2f;
                    foot.Rigidbody.SafeAddForce(force, ForceMode.Force);
                    RagdollMovement.AlignToVector(thigh, thigh.Transform.up, -player.Controls.WalkDir + Vector3.down, rigidity);
                    RagdollMovement.AlignToVector(leg, thigh.Transform.up, -player.Controls.WalkDir + Vector3.down, rigidity);
                    return -force;
                }
                RagdollMovement.AlignToVector(thigh, thigh.Transform.up, -player.Controls.WalkDir + Vector3.down, rigidity);
                RagdollMovement.AlignToVector(leg, thigh.Transform.up, -player.Controls.WalkDir, rigidity);
            }
            return Vector3.zero;
        }

        private void RotateBall()
        {
            float ballSpeed = player.State != PlayerState.Run ? 1.2f : 2.5f;
            Vector3 inverseDir = new Vector3(player.Controls.WalkDir.z, 0, -player.Controls.WalkDir.x);
            ragdoll.Ball.Rigidbody.angularVelocity = ballSpeed / ballRadius * inverseDir;
        }

        private void AddWalkForce()
        {
            float speed = player.Speed;
            Vector3 force = player.Controls.WalkDir * speed;
            ragdoll.Ball.Rigidbody.SafeAddForce(force, ForceMode.Force);
            //CALCULATE GROUND CHECK
            //if (player.Grounded)
            //    player.GroundManager.DistributeForce(-force, ragdoll.Ball.Rigidbody.position);
            //ADD SEGMENT TO CALCULATE IF GRABBING SOMETHING
        }
    }
}

