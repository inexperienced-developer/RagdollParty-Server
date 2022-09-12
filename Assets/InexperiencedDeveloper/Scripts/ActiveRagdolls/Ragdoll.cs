using InexperiencedDeveloper.Utils.Log;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class Ragdoll : MonoBehaviour
    {
        private bool initialized; //For Multiplayer
        public float HandLength;

        [Tooltip("Amount of body parts found (should be 16, maybe 17 if ball for balancing)")]
        public int BodyPartsFound = 0;

        #region Body Part variables
        public BodySegment Head;
        public BodySegment Chest;
        public BodySegment Waist;
        public BodySegment Hips;
        public BodySegment LeftArm;
        public BodySegment LeftForearm;
        public BodySegment LeftHand;
        public BodySegment RightArm;
        public BodySegment RightForearm;
        public BodySegment RightHand;
        public BodySegment LeftThigh;
        public BodySegment LeftLeg;
        public BodySegment LeftFoot;
        public BodySegment RightThigh;
        public BodySegment RightLeg;
        public BodySegment RightFoot;
        public BodySegment Ball; //For stability
        #endregion

        private void Awake()
        {
            if (initialized) return;

            initialized = true;

            //Get and configure all pieces of the ragdoll
            GetSegments();
            //Remove collision within the object
            SetupColliders();
            HandLength = (LeftArm.Transform.position - LeftForearm.Transform.position).magnitude + (LeftForearm.Transform.position - LeftHand.Transform.position).magnitude;
        }

        private void GetSegments()
        {
            Dictionary<string, Transform> dict = new();
            Transform[] transforms = GetComponentsInChildren<Transform>();
            //Organize all children into dict for easy access by name
            for (int i = 0; i < transforms.Length; i++)
                dict.Add(transforms[i].name.ToLower(), transforms[i]);
            Head = FindSegment(dict, "head");
            Chest = FindSegment(dict, "chest");
            Waist = FindSegment(dict, "waist");
            Hips = FindSegment(dict, "hips");
            LeftArm = FindSegment(dict, "arm.l");
            LeftForearm = FindSegment(dict, "forearm.l");
            LeftHand = FindSegment(dict, "hand.l");
            RightArm = FindSegment(dict, "arm.r");
            RightForearm = FindSegment(dict, "forearm.r");
            RightHand = FindSegment(dict, "hand.r");
            LeftThigh = FindSegment(dict, "thigh.l");
            LeftLeg = FindSegment(dict, "leg.l");
            LeftFoot = FindSegment(dict, "foot.l");
            RightThigh = FindSegment(dict, "thigh.r");
            RightLeg = FindSegment(dict, "leg.r");
            RightFoot = FindSegment(dict, "foot.r");
            DebugLogger.Log($"Found {BodyPartsFound} body parts");
            DebugLogger.LogWarning($"TODO: Collision set up for body parts");
            SetupHeadComponents(Head);
            SetupBodyComponents(Chest);
            SetupBodyComponents(Waist);
            SetupBodyComponents(Hips);
            SetupLimbComponents(LeftArm);
            SetupLimbComponents(LeftForearm);
            SetupLimbComponents(LeftThigh);
            SetupLimbComponents(LeftLeg);
            SetupLimbComponents(RightArm);
            SetupLimbComponents(RightForearm);
            SetupLimbComponents(RightThigh);
            SetupLimbComponents(RightLeg);
            SetupFootComponents(LeftFoot);
            SetupFootComponents(RightFoot);
            SetupHandComponents(LeftHand);
            SetupHandComponents(RightHand);
            LeftHand.Sensor.OtherSide = RightHand.Sensor;
            RightHand.Sensor.OtherSide = LeftHand.Sensor;
            AddAntiStretch(LeftHand, Chest);
            Debug.LogWarning("Added AntiStretch left Hand");
            AddAntiStretch(RightHand, Chest);
            AddAntiStretch(LeftFoot, Hips);
            AddAntiStretch(RightFoot, Hips);
        }

        private void AddAntiStretch(BodySegment seg1, BodySegment seg2)
        {
            ConfigurableJoint joint = seg1.Rigidbody.gameObject.AddComponent<ConfigurableJoint>();
            ConfigurableJoint joint2 = joint;
            ConfigurableJointMotion configurableJointMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = configurableJointMotion;
            joint.yMotion = configurableJointMotion;
            joint.xMotion = configurableJointMotion;
            joint.linearLimit = new SoftJointLimit
            {
                limit = (seg1.Transform.position - seg2.Transform.position).magnitude
            };
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedBody = seg2.Rigidbody;
            joint.connectedAnchor = Vector3.zero;
        }

        public void BindBall(Transform ballTransform)
        {
            Ball = InitializeSegment(ballTransform);
            SpringJoint spring = Ball.Rigidbody.GetComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = Hips.Transform.InverseTransformPoint(transform.position + Vector3.up * ((Ball.Collider as SphereCollider).radius + spring.maxDistance));
            spring.connectedBody = Hips.Rigidbody;
            IgnoreBallCollision();
        }

        private BodySegment FindSegment(Dictionary<string, Transform> children, string name)
        {
            return InitializeSegment(children[name.ToLower()]);
        }

        private BodySegment InitializeSegment(Transform t)
        {
            BodySegment segment = new();
            segment.Transform = t;
            segment.Collider = t.GetComponent<Collider>();
            segment.Rigidbody = t.GetComponent<Rigidbody>();
            segment.StartRot = t.localRotation;

            //FOR DEBUG ONLY
            if(segment.Collider == null) DebugLogger.LogError($"{t.name} is missing Collider", true);
            if(segment.Rigidbody == null) DebugLogger.LogError($"{t.name} is missing Rigidbody", true);
            BodyPartsFound++;
            print(t.name);

            return segment;
        }
        
        private void SetupHeadComponents(BodySegment segment)
        {
            segment.Sensor = segment.Transform.gameObject.AddComponent<CollisionSensor>();
        }
        private void SetupBodyComponents(BodySegment segment)
        {
            segment.Sensor = segment.Transform.gameObject.AddComponent<CollisionSensor>();
        }
        private void SetupLimbComponents(BodySegment segment)
        {
            segment.Sensor = segment.Transform.gameObject.AddComponent<CollisionSensor>();
        }
        private void SetupHandComponents(BodySegment segment)
        {
            segment.Sensor = segment.Transform.gameObject.AddComponent<CollisionSensor>();
        }
        private void SetupFootComponents(BodySegment segment)
        {
            segment.Sensor = segment.Transform.gameObject.AddComponent<CollisionSensor>();
            segment.Sensor.GroundCheck = true;
        }

        private void SetupColliders()
        {
            //Chest
            Physics.IgnoreCollision(Chest.Collider, Head.Collider);
            Physics.IgnoreCollision(Chest.Collider, LeftArm.Collider);
            Physics.IgnoreCollision(Chest.Collider, LeftForearm.Collider);
            Physics.IgnoreCollision(Chest.Collider, RightArm.Collider);
            Physics.IgnoreCollision(Chest.Collider, RightForearm.Collider);
            Physics.IgnoreCollision(Chest.Collider, Waist.Collider);

            //Hips
            Physics.IgnoreCollision(Hips.Collider, Chest.Collider);
            Physics.IgnoreCollision(Hips.Collider, Waist.Collider);
            Physics.IgnoreCollision(Hips.Collider, LeftThigh.Collider);
            Physics.IgnoreCollision(Hips.Collider, LeftLeg.Collider);
            Physics.IgnoreCollision(Hips.Collider, LeftFoot.Collider);
            Physics.IgnoreCollision(Hips.Collider, RightThigh.Collider);
            Physics.IgnoreCollision(Hips.Collider, RightLeg.Collider);
            Physics.IgnoreCollision(Hips.Collider, RightFoot.Collider);

            //Left Arm
            Physics.IgnoreCollision(LeftArm.Collider, LeftForearm.Collider);
            Physics.IgnoreCollision(LeftArm.Collider, LeftHand.Collider);
            Physics.IgnoreCollision(LeftForearm.Collider, LeftHand.Collider);
            
            //Right Arm
            Physics.IgnoreCollision(RightArm.Collider, RightForearm.Collider);
            Physics.IgnoreCollision(RightArm.Collider, RightHand.Collider);
            Physics.IgnoreCollision(RightForearm.Collider, RightHand.Collider);

            //Left Leg
            Physics.IgnoreCollision(LeftThigh.Collider, LeftLeg.Collider);

            //Right Leg
            Physics.IgnoreCollision(RightThigh.Collider, RightLeg.Collider);
        }

        private void IgnoreBallCollision()
        {
            Physics.IgnoreCollision(Ball.Collider, RightFoot.Collider);
            Physics.IgnoreCollision(Ball.Collider, RightLeg.Collider);
            Physics.IgnoreCollision(Ball.Collider, LeftFoot.Collider);
            Physics.IgnoreCollision(Ball.Collider, LeftLeg.Collider);
            Physics.IgnoreCollision(Ball.Collider, RightHand.Collider);
            Physics.IgnoreCollision(Ball.Collider, RightForearm.Collider);
            Physics.IgnoreCollision(Ball.Collider, RightArm.Collider);
            Physics.IgnoreCollision(Ball.Collider, LeftArm.Collider);
            Physics.IgnoreCollision(Ball.Collider, LeftForearm.Collider);
            Physics.IgnoreCollision(Ball.Collider, LeftHand.Collider);
            Physics.IgnoreCollision(Ball.Collider, Hips.Collider);
            Physics.IgnoreCollision(Ball.Collider, Chest.Collider);
            Physics.IgnoreCollision(Ball.Collider, Waist.Collider);
        }
    }
}

