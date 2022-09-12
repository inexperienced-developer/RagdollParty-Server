using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public enum TargetingMode
    {
        Shoulder,
        Chest,
        Hips,
        Ball
    }

    public class ArmMuscles
    {
        private Player player;
        private Ragdoll ragdoll;
        private RagdollMovement movement;

        public TargetingMode TargetingMode;
        public TargetingMode GrabTargetingMode = TargetingMode.Ball;
        private ScanMem LeftMem = new ScanMem();
        private ScanMem RightMem = new ScanMem();

        public ArmMuscles(Player player, Ragdoll ragdoll, RagdollMovement movement)
        {
            this.player = player;
            this.ragdoll = ragdoll;
            this.movement = movement;
        }

        public void OnFixedUpdate()
        {
            float targetPitchAngle = player.Controls.TargetPitchAngle;
            float targetYawAngle = player.Controls.TargetYawAngle;
            float leftExtend = 0.5f;
            float rightExtend = 0.5f;
            bool leftGrab = false;
            bool rightGrab = false;
            bool grounded = player.Grounded;
            if ((ragdoll.LeftHand.Transform.position - ragdoll.Chest.Transform.position).sqrMagnitude > 6f)
            {
                leftGrab = false;
            }
            if ((ragdoll.RightHand.Transform.position - ragdoll.Chest.Transform.position).sqrMagnitude > 6f)
            {
                rightGrab = false;
            }
            //GO THROUGH AND FIGURE THESE OUT
            Quaternion rot = Quaternion.Euler(targetPitchAngle, targetYawAngle, 0f);
            Quaternion rot2 = Quaternion.Euler(0, targetYawAngle, 0);
            Vector3 leftWorldPos = Vector3.zero;
            Vector3 rightWorldPos = Vector3.zero;
            float num = 0f;
            float z = 0f;
            if (targetPitchAngle > 0f && grounded)
            {
                z = 0.4f * targetPitchAngle / 90f;
            }
            TargetingMode leftHandTargetMode = (!(ragdoll.LeftHand.Sensor.GrabJoint != null)) ? TargetingMode : GrabTargetingMode;
            TargetingMode rightHandTargetMode = (!(ragdoll.RightHand.Sensor.GrabJoint != null)) ? TargetingMode : GrabTargetingMode;
            switch (leftHandTargetMode)
            {
                case TargetingMode.Shoulder:
                    leftWorldPos = ragdoll.LeftArm.Transform.position + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Chest:
                    leftWorldPos = ragdoll.Chest.Transform.position + rot2 * new Vector3(-0.2f, 0.15f, 0f) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Hips:
                    if (targetPitchAngle > 0f)
                    {
                        num = -0.3f * targetPitchAngle / 90f;
                    }
                    leftWorldPos = ragdoll.Hips.Transform.position + rot2 * new Vector3(-0.2f, 0.65f + num, z) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Ball:
                    if (targetPitchAngle > 0f)
                    {
                        num = -0.2f * targetPitchAngle / 90f;
                    }
                    if(ragdoll.LeftHand.Sensor.GrabJoint != null)
                    {
                        z = ((player.IsClimbing) ? 0f : -0.2f);
                    }
                    leftWorldPos = ragdoll.Ball.Transform.position + rot2 * new Vector3(-0.2f, 0.7f + num, z) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
            }
            switch (rightHandTargetMode)
            {
                case TargetingMode.Shoulder:
                    leftWorldPos = ragdoll.LeftArm.Transform.position + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Chest:
                    leftWorldPos = ragdoll.Chest.Transform.position + rot2 * new Vector3(0.2f, 0.15f, 0f) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Hips:
                    if (targetPitchAngle > 0f)
                    {
                        num = -0.3f * targetPitchAngle / 90f;
                    }
                    leftWorldPos = ragdoll.Hips.Transform.position + rot2 * new Vector3(0.2f, 0.65f + num, z) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
                case TargetingMode.Ball:
                    if (targetPitchAngle > 0f)
                    {
                        num = -0.2f * targetPitchAngle / 90f;
                    }
                    if (ragdoll.LeftHand.Sensor.GrabJoint != null)
                    {
                        z = ((player.IsClimbing) ? 0f : -0.2f);
                    }
                    leftWorldPos = ragdoll.Ball.Transform.position + rot2 * new Vector3(0.2f, 0.7f + num, z) + rot * new Vector3(0f, 0f, leftExtend * ragdoll.HandLength);
                    break;
            }
            ProcessHand(LeftMem, ragdoll.LeftArm, ragdoll.LeftForearm, ragdoll.LeftHand, leftWorldPos, leftExtend, leftGrab, movement.Legs.LegPhase + 0.5f, false);
            ProcessHand(RightMem, ragdoll.RightArm, ragdoll.RightForearm, ragdoll.RightHand, rightWorldPos, rightExtend, rightGrab, movement.Legs.LegPhase, true);
        }

        private void ProcessHand(ScanMem mem, BodySegment arm, BodySegment forearm, BodySegment hand, Vector3 worldPos, float extend, bool grab, float animPhase, bool right)
        {
            double num = 0.1 + (double)(0.14f * Mathf.Abs(player.Controls.TargetPitchAngle - mem.GrabAngle / 80f));
            double num2 = num * 2.0;
            if(grab && !hand.Sensor.Grab)
            {
                if ((double)mem.GrabTime > num)
                {
                    mem.Pos = arm.Transform.position;
                }
                else
                {
                    grab = false;
                }
            }
            if(hand.Sensor.Grab && !grab)
            {
                mem.GrabTime = 0f;
                mem.GrabAngle = player.Controls.TargetPitchAngle;
            }
            else
            {
                mem.GrabTime += Time.fixedDeltaTime;
            }
            hand.Sensor.Grab = ((double)mem.GrabTime > num2 && grab);
            if(extend > 0.2f)
            {
                hand.Sensor.TargetPos = worldPos;
                mem.Shoulder = arm.Transform.position;
                mem.Hand = hand.Transform.position;
                if(hand.Sensor.GrabJoint == null)
                {
                    worldPos = FindTarget(mem, worldPos, out hand.Sensor.GrabFilter);
                }
                PlaceHand(arm, hand, worldPos, true, hand.Sensor.GrabJoint != null, hand.Sensor.GrabbedRB);
                if(hand.Sensor.GrabbedRB != null)
                {
                    LiftBody(hand, hand.Sensor.GrabbedRB);
                }
                hand.Sensor.GrabPos = worldPos;
            }
            else
            {
                hand.Sensor.GrabFilter = null;
                if(player.State == PlayerState.Run)
                {
                    AnimateHand(arm, forearm, hand, animPhase, 1f, right);
                }
                //else if(player.State == PlayerState.Freefall)
                else
                {
                    Vector3 targetDir = player.TargetDir;
                    targetDir.y = 0f;
                    RagdollMovement.AlignToVector(arm, arm.Transform.up, -targetDir, 20f);
                    RagdollMovement.AlignToVector(forearm, forearm.Transform.up, targetDir, 20f);
                }
            }
        }

        private void AnimateHand(BodySegment arm, BodySegment forearm, BodySegment hand, float phase, float rigidity, bool right)
        {
            rigidity *= 50f * player.Controls.WalkSpeed;
            phase -= Mathf.Floor(phase);
            Vector3 a = Quaternion.Euler(0, player.Controls.TargetYawAngle, 0) * Vector3.forward;
            Vector3 vector = Quaternion.Euler(0, player.Controls.TargetYawAngle, 0) * Vector3.right;
            if (!right) vector = -vector;
            if (phase < 0.5f)
            {
                RagdollMovement.AlignToVector(arm, arm.Transform.up, Vector3.down + vector / 2f, 3f * rigidity);
                RagdollMovement.AlignToVector(forearm, forearm.Transform.up, a / 2f - vector, 3f * rigidity);
            }
            else
            {
                RagdollMovement.AlignToVector(arm, arm.Transform.up, -a + vector / 2f, 3f * rigidity);
                RagdollMovement.AlignToVector(forearm, forearm.Transform.up, a + Vector3.down, 3f * rigidity);
            }

        }

        private void PlaceHand(BodySegment arm, BodySegment hand, Vector3 worldPos, bool active, bool grabbed, Rigidbody grabbedRB)
        {

        }

        private void LiftBody(BodySegment hand, Rigidbody rb)
        {

        }

        private Vector3 FindTarget(ScanMem mem, Vector3 worldPos, out Collider target)
        {
            target = new Collider();
            return Vector3.zero;
        }

        private class ScanMem
        {
            public Vector3 Pos;
            public Vector3 Shoulder;
            public Vector3 Hand;
            public float GrabTime;
            public float GrabAngle;
        }
    }

}

