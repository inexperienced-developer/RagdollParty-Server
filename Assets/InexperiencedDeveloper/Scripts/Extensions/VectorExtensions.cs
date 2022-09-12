using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ZeroY(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
    }
}

