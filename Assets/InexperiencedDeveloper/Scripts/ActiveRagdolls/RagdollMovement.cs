using InexperiencedDeveloper.Core;
using InexperiencedDeveloper.Extensions;
using InexperiencedDeveloper.Utils.Log;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class RagdollMovement : MonoBehaviour
    {
        private Player Player;
        private Ragdoll Ragdoll;
        public TorsoMuscles Torso;
        public LegMuscles Legs;
        public ArmMuscles Arms;

        public void Init()
        {
            if (Player != null) return;
            Player = GetComponent<Player>();
            Ragdoll = Player.Ragdoll;
            Torso = new TorsoMuscles(Player, Ragdoll, this);
            Legs = new LegMuscles(Player, Ragdoll, this);
            Arms = new ArmMuscles(Player, Ragdoll, this);
        }

        public void OnFixedUpdate()
        {
            //ADD HANDS
            if(Torso != null && Legs != null && Arms != null)
            {
                Arms.OnFixedUpdate();
                Torso.OnFixedUpdate();
                Legs.OnFixedUpdate(Torso.FeedbackForce);
            }
            else
            {
                Init();
            }
        }

        #region Alignment
        public static void AlignToVector(BodySegment part, Vector3 alignmentVector, Vector3 target, float spring)
        {
            AlignToVector(part.Rigidbody, alignmentVector, target, spring, spring);
        }

        public static void AlignToVector(Rigidbody rb, Vector3 alignmentVector, Vector3 target, float spring, float maxTorque)
        {
            float multiplier = 0.1f;
            //57.29578 is 1 radian in degrees
            Vector3 cross = Vector3.Cross((Quaternion.AngleAxis(rb.angularVelocity.magnitude * 57.29578f * multiplier, rb.angularVelocity) * alignmentVector.normalized).normalized, target.normalized);
            Vector3 align = cross.normalized * Mathf.Asin(Mathf.Clamp01(cross.magnitude));
            align *= spring;
            rb.SafeAddTorque(Vector3.ClampMagnitude(align, maxTorque), ForceMode.Force);
        }

        public static void AlignLook(BodySegment part, Quaternion targetRot, float spring, float damping)
        {
            float angle;
            Vector3 axis;
            (targetRot * Quaternion.Inverse(part.Transform.rotation)).ToAngleAxis(out angle, out axis);
            if (angle > 180)
                angle -= 360;
            if (angle < 180)
                angle += 360;
            part.Rigidbody.SafeAddTorque(axis * angle * spring - part.Rigidbody.angularVelocity * damping, ForceMode.Acceleration);
        }
        #endregion
    }
}

