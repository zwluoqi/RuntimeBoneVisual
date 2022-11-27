using UnityEngine;

namespace Content.Scritps.Charactor
{
    public class CachePreTrans
    {
        public Transform cache;
        private Vector3 prePos;
        private Vector3 preRotation;
        private Vector3 preScale;

        public CachePreTrans(Transform _trans)
        {
            this.cache = _trans;
            SaveTransform(out var deltaPos, out var deltaEuler, out var deltaScale);
        }

        public bool TransformChanged()
        {
            return TransformChanged(out var deltaPos, out var deltaEuler, out var deltaScale);
        }
        public bool TransformChanged(out Vector3 deltaPos,out Vector3 deltaEuler,out Vector3 deltaScale)
        {
            if (cache.position != prePos)
            {
                SaveTransform(out deltaPos,out deltaEuler,out deltaScale);
                return true;
            }
            if (cache.eulerAngles != preRotation)
            {
                SaveTransform(out deltaPos,out deltaEuler,out deltaScale);
                return true;
            }
            if (cache.localScale != preScale)
            {
                SaveTransform(out deltaPos,out deltaEuler,out deltaScale);
                return true;
            }
            deltaPos = Vector3.zero;
            deltaEuler = Vector3.zero;
            deltaScale = Vector3.zero;
            return false;
        }

        public void SaveTransform()
        {
            SaveTransform(out var deltaPos, out var deltaEuler, out var deltaScale);
        }
        private void SaveTransform(out Vector3 deltaPos,out Vector3 deltaEuler,out Vector3 deltaScale)
        {
            var transform1 = cache;
            deltaPos = cache.position - prePos;
            deltaEuler = cache.eulerAngles - preRotation;
            deltaScale = cache.localScale - preScale;
            
            prePos = transform1.position;
            preRotation = transform1.eulerAngles;
            preScale = transform1.localScale;
        }
    }
}