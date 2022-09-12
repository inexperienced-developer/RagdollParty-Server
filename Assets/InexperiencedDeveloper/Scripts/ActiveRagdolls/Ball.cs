using InexperiencedDeveloper.ActiveRagdoll;
using InexperiencedDeveloper.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Player player;
    private Ragdoll ragdoll;
    private float ballRadius;
    private GrabManager grabManager;

    private List<Collision> collisions;
    private List<Vector3> contacts;

    public LayerMask CollisionLayers;
    public float TimeSinceLastNonZeroImpulse;

    private void OnEnable()
    {
        player = GetComponent<Player>();
        ragdoll = GetComponent<Ragdoll>();
        ballRadius = GetComponent<SphereCollider>().radius;
        grabManager = GetComponent<GrabManager>();
    }

    private void FixedUpdate()
    {
        collisions.Clear();
        contacts.Clear();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length == 0)
            return;
        HandleCollision(collision);
        collisions.Add(collision);
        for(int i = 0; i < collision.contacts.Length; i++)
        {
            contacts.Add(collision.contacts[i].point);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.contacts.Length == 0)
            return;
        HandleCollision(collision);
        collisions.Add(collision);
        for (int i = 0; i < collision.contacts.Length; i++)
            contacts.Add(collision.contacts[i].point);
    }

    private void HandleCollision(Collision collision)
    {
        Vector3 impulse = collision.GetImpulse();
        if (impulse.y > 0f && player.Grounded)
            TimeSinceLastNonZeroImpulse = Time.time;
        Vector3 walkDir = player.Controls.WalkDir;
        if(Vector3.Dot(impulse, walkDir) >= 0f)
            return;
        float impulseMod = 0f;
        for(int i = 0; i < collision.contacts.Length; i++)
        {
            Vector3 contactPoint = collision.contacts[i].point;
            Vector3 upAdjPoint = contactPoint + walkDir * 0.07f + Vector3.up * 0.07f;
            Vector3 downAdjPoint = contactPoint - walkDir * 0.07f - Vector3.up * 0.07f;
            RaycastHit hit;
            if (Physics.Raycast(upAdjPoint, Vector3.down, out hit, 0.1f, this.CollisionLayers) && hit.normal.y >0.7f &&
                Physics.Raycast(downAdjPoint, walkDir, out hit, 0.1f, this.CollisionLayers) && hit.normal.y < 0.4f)
            {
                Debug.DrawLine(transform.position, collision.contacts[i].point, Color.red);
                impulseMod = 1.5f;
                break;
            }
        }
        if (ragdoll.LeftHand.Sensor.GrabJoint != null && ragdoll.RightHand.Sensor.GrabJoint != null)
        {
            float leftJointCounterForce = (!(ragdoll.LeftHand.Sensor.GrabJoint != null)) ? 0f : Vector3.Dot(ragdoll.LeftHand.Transform.position - transform.position, walkDir);
            float rightJointBreakForce = (!(ragdoll.RightHand.Sensor.GrabJoint != null)) ? 0f : Vector3.Dot(ragdoll.RightHand.Transform.position - transform.position, walkDir);
            impulseMod = Mathf.Max(impulseMod, (leftJointCounterForce + rightJointBreakForce) / 2f);
        }
        if (impulseMod > 0f)
        {
            Vector3 adjImpulse = impulse.ZeroY();
            impulse = Vector3.up * adjImpulse.magnitude * impulseMod - adjImpulse / 2f;
            ragdoll.Ball.Rigidbody.SafeAddForce(impulse, ForceMode.Impulse);
            player.GroundManager.DistributeForce(-impulse / Time.fixedDeltaTime, transform.position);
        }
    }
}
