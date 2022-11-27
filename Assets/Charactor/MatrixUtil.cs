using UnityEngine;

namespace Content.Scritps.Charactor
{
    public static class MatrixUtil
    {
        public static Vector3 GetT(this Matrix4x4 trs)
        {
            return trs.GetColumn(3);
        }

        public static Quaternion GetR(this Matrix4x4 trs)
        {
            return Quaternion.LookRotation(trs.GetColumn(2), trs.GetColumn(1));
        }

        public static Vector3 GetS(this Matrix4x4 trs)
        {
            return new Vector3(trs.GetColumn(0).magnitude, trs.GetColumn(1).magnitude, trs.GetColumn(2).magnitude);
        }
    }
}