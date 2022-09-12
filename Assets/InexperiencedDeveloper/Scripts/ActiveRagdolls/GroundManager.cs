using InexperiencedDeveloper.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class GroundManager : MonoBehaviour
    {
        private static List<GroundManager> all = new List<GroundManager>();
        public List<GameObject> GroundObjects = new List<GameObject>();
        private List<Rigidbody> groundRigids = new List<Rigidbody>();

        public Vector3 GroundSpeed
        {
            get
            {
                Vector3 vel = Vector3.zero;
                for(int i = 0; i < groundRigids.Count; i++)
                {
                    Rigidbody rb = groundRigids[i];
                    if(rb != null)
                    {
                        Vector3 rbVel = rb.velocity;
                        if (Mathf.Abs(vel.x) < Mathf.Abs(rbVel.x))
                            vel.x = rbVel.x;
                        if (Mathf.Abs(vel.y) < Mathf.Abs(rbVel.y))
                            vel.y = rbVel.y;
                        if (Mathf.Abs(vel.z) < Mathf.Abs(rbVel.z))
                            vel.z = rbVel.z;
                    }
                }
                return vel;
            }
        }

        private void OnEnable()
        {
            all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        public void DistributeForce(Vector3 force, Vector3 pos)
        {
            for (int i = 0; i < groundRigids.Count; i++)
            {
                Rigidbody rb = groundRigids[i];
                if (rb != null)
                    rb.SafeAddForceAtPosition(Vector3.ClampMagnitude(force / (float)groundRigids.Count, rb.mass / Time.fixedDeltaTime * 10f), pos, ForceMode.Force);
            }
        }
    }
}

