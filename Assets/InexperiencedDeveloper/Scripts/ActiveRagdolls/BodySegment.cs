using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class BodySegment
    {
        public Transform Transform;
        public Collider Collider;
        public Rigidbody Rigidbody;
        public CollisionSensor Sensor;
        public Quaternion StartRot;
        public Transform Skele;
        public BodySegment Parent;
    }
}

