using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.ActiveRagdoll
{
    public class GrabManager : MonoBehaviour
    {
        public List<GameObject> GrabbedObjs = new();

        private static Dictionary<GameObject, Vector3> GrabStartPositions = new();
        private static List<GrabManager> all = new();
        private Player player;

        private void OnEnable()
        {
            all.Add(this);
            player = GetComponentInParent<Player>();
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        public void ObjGrabbed(GameObject grabObj)
        {
            bool flag = true;
            for(int i= 0; i < all.Count; i++)
            {
                flag &= !all[i].GrabbedObjs.Contains(grabObj);
            }
            GrabbedObjs.Add(grabObj);
            if (flag)
            {
                IGrabbable grabbable = grabObj.GetComponentInParent<IGrabbable>();
                if (grabbable != null)
                {
                    grabbable.OnGrab();
                }
                IGrabbableWithInfo grabbableInfo = grabObj.GetComponentInParent<IGrabbableWithInfo>();
                if (grabbableInfo != null)
                {
                    grabbableInfo.OnGrab(this);
                }
                GrabStartPositions[grabObj] = grabObj.transform.position;
                Player otherPlayer = grabObj.GetComponentInParent<Player>();
                if(otherPlayer != null)
                {
                    otherPlayer.GrabbedByPlayer = player;
                }
            }
        }
    }
}

