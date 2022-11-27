using System;
using System.Collections.Generic;
using RuntimeGizmos;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

namespace Content.Scritps.Charactor
{
    /// <summary>
    /// The BoneRenderer component is responsible for displaying pickable bones in the Scene View.
    /// This component does nothing during runtime.
    /// </summary>
    [ExecuteAlways]
    public class VRBoneRenderer : MonoBehaviour
    {
        /// <summary>
        /// Shape used by individual bones.
        /// </summary>
        public enum BoneShape
        {
            /// <summary>Bones are rendered with single lines.</summary>
            Line,

            /// <summary>Bones are rendered with pyramid shapes.</summary>
            Pyramid,

            /// <summary>Bones are rendered with box shapes.</summary>
            Box
        };

        /// <summary>Shape of the bones.</summary>
        public BoneShape boneShape = BoneShape.Pyramid;

        /// <summary>Toggles whether to render bone shapes or not.</summary>
        public bool drawBones = true;

        /// <summary>Toggles whether to draw tripods on bones or not.</summary>
        public bool drawTripods = false;

        /// <summary>Size of the bones.</summary>
        [Range(0.01f, 5.0f)] public float boneSize = 1.0f;

        /// <summary>Size of the tripod axis.</summary>
        [Range(0.01f, 5.0f)] public float tripodSize = 1.0f;

        /// <summary>Color of the bones.</summary>
        public Color boneColor = new Color(0f, 0f, 1f, 0.5f);

        [SerializeField] private Transform[] m_Transforms;

        /// <summary>Transform references in the BoneRenderer hierarchy that are used to build bones.</summary>
        public Transform[] transforms
        {
            get { return m_Transforms; }
// #if UNITY_EDITOR
            set
            {
                m_Transforms = value;
                ExtractBones();
            }
// #endif
        }
        
        /// <summary>
        /// Bone described by two Transform references.
        /// </summary>
        public struct TransformPair
        {
            public Transform first;
            public Transform second;
        };

        private TransformPair[] m_Bones;
        private Transform[] m_Tips;

        /// <summary>Retrieves the bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public TransformPair[] bones
        {
            get => m_Bones;
        }

        /// <summary>Retrieves the tip bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public Transform[] tips
        {
            get => m_Tips;
        }

        public static Action<bool> OnSelectBone;
        private CachePreTrans _cachePreTran;
        private CachePreTrans _hipPreTran;
        private void Awake()
        {
            _cachePreTran = new CachePreTrans(this.transform);
            InitBones();
        }

        void OnEnable()
        {
            ExtractBones();
            // onAddBoneRenderer?.Invoke(this);
            VRBoneRendererUtils.OnAddBoneRenderer(this);
            VRBoneRendererUtils.DrawSkeletons();
            TransformGizmo.OnHitObject += OnHitObject;
            TransformGizmo.OnAddTargetRoot += OnAddTargetRoot;
            
            TransformGizmo.OnClearObject += OnClearSelectObject;
        }


        void OnDisable()
        {
            TransformGizmo.OnHitObject -= OnHitObject;
            TransformGizmo.OnClearObject -= OnClearSelectObject;
            TransformGizmo.OnAddTargetRoot -= OnAddTargetRoot;
            // onRemoveBoneRenderer?.Invoke(this);
            VRBoneRendererUtils.OnRemoveBoneRenderer(this);
            VRBoneRendererUtils.DrawSkeletons();
        }

        private void OnDestroy()
        {
            OnClearSelectObject();
            foreach (var col in colliders)
            {
                DestroyImmediate(col.collider.gameObject);
            }

            colliders = null;
            if (colliderParent)
            {
                DestroyImmediate(colliderParent.gameObject);
            }
        }


        private CachePreTrans curSelectBone;
        private Transform OnHitObject(Transform collider)
        {
            if (!cacheObjects.TryGetValue(collider.name,out var boneCollider))
            {
                return null;
            }
            return boneCollider.linked;
        }
        
        private void OnClearSelectObject()
        {
            OnSelectBone?.Invoke(false);
            curSelectBone = null;
            
        }
        
        
        private void OnAddTargetRoot(Transform bone)
        {
            curSelectBone = new CachePreTrans(bone);
            OnSelectBone?.Invoke(true);
        }

        private void Update()
        {
            bool transformChanged = _cachePreTran != null && _cachePreTran.TransformChanged();
            bool hipBoneformChanged = _hipPreTran != null && _hipPreTran.TransformChanged();
            bool selectBoneformChanged = curSelectBone != null && curSelectBone.TransformChanged();
            
            if (transformChanged || hipBoneformChanged || selectBoneformChanged)
            {
                VRBoneRendererUtils.DrawSkeletons();
            }

            if (this.drawTripods)
            {
                DrawTripods();
            }
            VRBoneRendererUtils.lineMeshRender.Render();
            // if (Input.GetKeyDown(KeyCode.T))
            // {
            //     Util.EnforceTPose(this.GetComponent<Animator>());
            // }
        }


        private void DrawTripods()
        {
            var size = this.tripodSize * 0.025f;
            for (var j = 0; j < this.transforms.Length; j++)
            {
                var tripodSize = 1f;
                var transform = this.transforms[j];
                if (transform == null)
                    continue;

                var position = transform.position;
                var xAxis = position + transform.rotation * Vector3.right * size * tripodSize;
                var yAxis = position + transform.rotation * Vector3.up * size * tripodSize;
                var zAxis = position + transform.rotation * Vector3.forward * size * tripodSize;

                // Handles.color = Color.red;
                Gizmos.Line(position, xAxis,Color.red,true);
                // Handles.color = Color.green;
                Gizmos.Line(position, yAxis,Color.green,true);
                // Handles.color = Color.blue;
                Gizmos.Line(position, zAxis,Color.blue,true);
            }
        }


        public void OnValidate()
        {
            ExtractBones();
            VRBoneRendererUtils.DrawSkeletons();
        }

        /// <summary>
        /// Resets the BoneRenderer to default values.
        /// </summary>
        public void Reset()
        {
            ClearBones();
        }

        /// <summary>
        /// Clears bones and tip bones.
        /// </summary>
        public void ClearBones()
        {
            m_Bones = null;
            m_Tips = null;
        }


        public void InitBones()
        {
            var animator = transform.GetComponent<Animator>();
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            _hipPreTran = new CachePreTrans(hips.transform);

            var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bones = new List<Transform>();
            if (animator != null && renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var renderer = renderers[i];
                    for (int j = 0; j < renderer.bones.Length; ++j)
                    {
                        var bone = renderer.bones[j];
                        if (!bones.Contains(bone))
                        {
                            bones.Add(bone);

                            for (int k = 0; k < bone.childCount; k++)
                            {
                                if (!bones.Contains(bone.GetChild(k)))
                                    bones.Add(bone.GetChild(k));
                            }
                        }
                    }
                }
            }
            else
            {
                bones.AddRange(transform.GetComponentsInChildren<Transform>());
            }

            this.transforms = bones.ToArray();
            InitJointCollider();
        }



        /// <summary>
        /// Builds bones and tip bones from Transform references.
        /// </summary>
        public void ExtractBones()
        {
            if (m_Transforms == null || m_Transforms.Length == 0)
            {
                ClearBones();
                return;
            }

            var transformsHashSet = new HashSet<Transform>(m_Transforms);

            var bonesList = new List<TransformPair>(m_Transforms.Length);
            var tipsList = new List<Transform>(m_Transforms.Length);

            for (int i = 0; i < m_Transforms.Length; ++i)
            {
                bool hasValidChildren = false;

                var transform = m_Transforms[i];
                if (transform == null)
                    continue;

                // if (UnityEditor.SceneVisibilityManager.instance.IsHidden(transform.gameObject, false))
                //     continue;
                //
                // var mask = UnityEditor.Tools.visibleLayers;
                // if ((mask & (1 << transform.gameObject.layer)) == 0)
                //     continue;

                if (transform.childCount > 0)
                {
                    for (var k = 0; k < transform.childCount; ++k)
                    {
                        var childTransform = transform.GetChild(k);

                        if (transformsHashSet.Contains(childTransform))
                        {
                            bonesList.Add(new TransformPair() {first = transform, second = childTransform});
                            hasValidChildren = true;
                        }
                    }
                }

                if (!hasValidChildren)
                {
                    tipsList.Add(transform);
                }
            }

            m_Bones = bonesList.ToArray();
            m_Tips = tipsList.ToArray();
            
            cacheObjects.Clear();
            if (colliderParent != null)
            {
                foreach (var col in colliders)
                {
                    cacheObjects[col.collider.name] = col;
                }
            }
        }
        
        private void InitJointCollider()
        {
            if (colliderParent != null)
            {
                foreach (var col in colliders)
                {
                    cacheObjects[col.collider.name] = col;
                }
            }
            else
            {
                List<RenderCollider> inited = new List<RenderCollider>();
                foreach (var bone in this.bones)
                {
                    inited.Add(AddMeshCollider(bone.first,bone.second));
                }

                colliders = inited.ToArray();
            }
        }

        public RenderCollider[] colliders;
        public Transform colliderParent;
        Dictionary<string,RenderCollider> cacheObjects = new Dictionary<string, RenderCollider>();

        public static Material material0;
        public static Material material1;
        private RenderCollider AddMeshCollider(Transform first,Transform second)
        {
            if (colliderParent == null)
            {
                colliderParent = (new GameObject()).transform;
                colliderParent.SetParent(this.transform);
            }

            if (material0 == null)
            {
                material0 = Resources.Load<Material>("Bone/Unlit");
            }
            
            if (material1 == null)
            {
                // material1 = Resources.Load<Material>("Bone/Unlit_WIRE_ON");
                material1 = Instantiate(material0);
                material1.color = new Color(material1.color.r, material1.color.g, material1.color.b, 1);
            }

            string jointName = GetJointName(first, second);
            if (!cacheObjects.TryGetValue(jointName, out var renderColldier)
                || renderColldier.collider == null)
            {
                renderColldier = new RenderCollider();
                var collider = (new GameObject()).transform;
                collider.name = jointName;
                collider.SetParent(colliderParent);
                renderColldier.collider = collider.gameObject.AddComponent<MeshCollider>();
                
                //INSTANCE NOT SUPPORT
                collider.gameObject.AddComponent<MeshFilter>();
                var meshRenderer = collider.gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = new[] {material0,material1};
                //INSTANCE NOT SUPPORT
                
                renderColldier.linked = first;
                cacheObjects[jointName] = renderColldier;
            }

            return renderColldier;
        }
        public void UpdateMeshCollider(Transform first,Transform second,Matrix4x4 matrix4X4,Mesh totalMesh,Mesh mesh)
        {
            if (colliderParent == null)
            {
                InitJointCollider();
            }

            string jointName = GetJointName(first, second); 
            var renderColldier = cacheObjects[jointName];
            renderColldier.collider.sharedMesh = mesh;
            renderColldier.collider.transform.position = matrix4X4.GetT();
            renderColldier.collider.transform.rotation = matrix4X4.GetR();
            renderColldier.collider.transform.localScale = matrix4X4.GetS();
            
            //NOT SUPPORT INSTANCE
            renderColldier.collider.GetComponent<MeshFilter>().sharedMesh = totalMesh;
            //NOT SUPPORT INSTANCE
        }

        private string GetJointName(Transform first, Transform second)
        {
            if (second == null)
            {
                return "collider_" + first.name + "-null";
            }
            else
            {
                return "collider_" + first.name + "-" + second.name;
            }
        }

        [System.Serializable]
        public class RenderCollider
        {
            public Transform linked;
            public MeshCollider collider;
        }
    }
}
