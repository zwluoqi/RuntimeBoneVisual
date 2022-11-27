using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Gizmos = Popcron.Gizmos;

namespace Content.Scritps.Charactor
{
    using BoneShape = VRBoneRenderer.BoneShape;

    static class VRBoneRendererUtils
    {
        

        static List<VRBoneRenderer> s_BoneRendererComponents = new List<VRBoneRenderer>();

        public static BatchRenderer s_PyramidMeshRenderer;
        public static BatchRenderer s_BoxMeshRenderer;

        private static Material s_Material;

        private const float k_Epsilon = 1e-5f;

        private const float k_BoneBaseSize = 2f;
        private const float k_BoneTipSize = 0.5f;

        private static int s_ButtonHash = "BoneHandle".GetHashCode();

        // private static int s_VisibleLayersCache = 0;

        // static VRBoneRendererUtils()
        // {
            // VRBoneRenderer.onAddBoneRenderer += OnAddBoneRenderer;
            // VRBoneRenderer.onRemoveBoneRenderer += OnRemoveBoneRenderer;
            // SceneVisibilityManager.visibilityChanged += OnVisibilityChanged;
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            //
            // SceneView.duringSceneGui += DrawSkeletons;

            // s_VisibleLayersCache = Tools.visibleLayers;
        // }

        private static Material material
        {
            get
            {
                if (!s_Material)
                {
                    Shader shader = Shader.Find("Hidden/VRBoneHandles");
                    s_Material = new Material(shader);
                    s_Material.hideFlags = HideFlags.DontSaveInEditor;
                    s_Material.enableInstancing = true;
                }

                return s_Material;
            }
        }

        public static BatchRenderer pyramidMeshRenderer
        {
            get
            {
                if (s_PyramidMeshRenderer == null)
                {
                    var mesh = new Mesh();
                    mesh.name = "BoneRendererPyramidMesh";
                    mesh.subMeshCount = (int)BatchRenderer.SubMeshType.Count;
                    mesh.hideFlags = HideFlags.DontSave;

                    // Bone vertices
                    Vector3[] vertices = new Vector3[]
                    {
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, -1.0f),
                        new Vector3(-0.9f, 0.0f, 0.5f),
                        new Vector3(0.9f, 0.0f, 0.5f),
                    };

                    mesh.vertices = vertices;

                    // Build indices for different sub meshes
                    int[] boneFaceIndices = new int[]
                    {
                        0, 2, 1,
                        0, 1, 3,
                        0, 3, 2,
                        1, 2, 3
                    };
                    mesh.SetIndices(boneFaceIndices, MeshTopology.Triangles, (int)BatchRenderer.SubMeshType.BoneFaces);

                    int[] boneWireIndices = new int[]
                    {
                        0, 1, 0, 2, 0, 3, 1, 2, 2, 3, 3, 1
                    };
                    mesh.SetIndices(boneWireIndices, MeshTopology.Lines, (int)BatchRenderer.SubMeshType.BoneWire);

                    s_PyramidMeshRenderer = new BatchRenderer()
                    {
                        mesh = mesh,
                        collideMesh = GetMesh(mesh,0),
                        material = material
                    };
                }

                return s_PyramidMeshRenderer;
            }
        }

        private static Mesh GetMesh(Mesh mesh, int i)
        {
            var newMesh = new Mesh();
            newMesh.name = "Collide_"+mesh.name;
            newMesh.subMeshCount = 1;
            newMesh.hideFlags = HideFlags.DontSave;
            newMesh.vertices = mesh.vertices;
            var indices = mesh.GetIndices(i);
            newMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            return newMesh;
        }

        public static BatchRenderer boxMeshRenderer
        {
            get
            {
                if (s_BoxMeshRenderer == null)
                {
                    var mesh = new Mesh();
                    mesh.name = "BoneRendererBoxMesh";
                    mesh.subMeshCount = (int)BatchRenderer.SubMeshType.Count;
                    mesh.hideFlags = HideFlags.DontSave;
                    

                    // Bone vertices
                    Vector3[] vertices = new Vector3[]
                    {
                        new Vector3(-0.5f, 0.0f, 0.5f),
                        new Vector3(0.5f, 0.0f, 0.5f),
                        new Vector3(0.5f, 0.0f, -0.5f),
                        new Vector3(-0.5f, 0.0f, -0.5f),
                        new Vector3(-0.5f, 1.0f, 0.5f),
                        new Vector3(0.5f, 1.0f, 0.5f),
                        new Vector3(0.5f, 1.0f, -0.5f),
                        new Vector3(-0.5f, 1.0f, -0.5f)
                    };

                    mesh.vertices = vertices;

                    // Build indices for different sub meshes
                    int[] boneFaceIndices = new int[]
                    {
                        0, 2, 1,
                        0, 3, 2,

                        0, 1, 5,
                        0, 5, 4,

                        1, 2, 6,
                        1, 6, 5,

                        2, 3, 7,
                        2, 7, 6,

                        3, 0, 4,
                        3, 4, 7,

                        4, 5, 6,
                        4, 6, 7
                    };
                    mesh.SetIndices(boneFaceIndices, MeshTopology.Triangles, (int)BatchRenderer.SubMeshType.BoneFaces);
                    
                    int[] boneWireIndices = new int[]
                    {
                        0, 1, 1, 2, 2, 3, 3, 0,
                        4, 5, 5, 6, 6, 7, 7, 4,
                        0, 4, 1, 5, 2, 6, 3, 7
                    };
                    mesh.SetIndices(boneWireIndices, MeshTopology.Lines, (int)BatchRenderer.SubMeshType.BoneWire);
                    
                    
                    s_BoxMeshRenderer = new BatchRenderer()
                    {
                        mesh = mesh,
                        collideMesh = GetMesh(mesh,0),
                        material = material
                    };

                }

                return s_BoxMeshRenderer;
            }
        }

        public static LineBatchRender lineMeshRender = new LineBatchRender();
        
        private static Matrix4x4 ComputeBoneMatrix(Vector3 start, Vector3 end, float length, float size,out float scale,out Vector3 direction)
        {
            direction = (end - start) / length;
            Vector3 tangent = Vector3.Cross(direction, Vector3.up);
            if (Vector3.SqrMagnitude(tangent) < 0.1f)
                tangent = Vector3.Cross(direction, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(direction, tangent);

            scale = length * k_BoneBaseSize * size;

            return new Matrix4x4(
                new Vector4(tangent.x   * scale,  tangent.y   * scale,  tangent.z   * scale , 0f),
                new Vector4(direction.x * length, direction.y * length, direction.z * length, 0f),
                new Vector4(bitangent.x * scale,  bitangent.y * scale,  bitangent.z * scale , 0f),
                new Vector4(start.x, start.y, start.z, 1f));
        }

        public static void DrawSkeletons()
        {
            // var gizmoColor = Gizmos.color;

            pyramidMeshRenderer.Clear();
            boxMeshRenderer.Clear();

            for (var i = 0; i < s_BoneRendererComponents.Count; i++)
            {
                var boneRenderer = s_BoneRendererComponents[i];

                if (boneRenderer.bones == null)
                    continue;

                // PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                // if (prefabStage != null)
                // {
                //     StageHandle stageHandle = prefabStage.stageHandle;
                //     if (stageHandle.IsValid() && !stageHandle.Contains(boneRenderer.gameObject))
                //         continue;
                // }

                if (boneRenderer.drawBones)
                {
                    var size = boneRenderer.boneSize * 0.025f;
                    var shape = boneRenderer.boneShape;
                    var color = boneRenderer.boneColor;
                    var nubColor = new Color(color.r, color.g, color.b, color.a);
                    var selectionColor = Color.white;

                    for (var j = 0; j < boneRenderer.bones.Length; j++)
                    {
                        var bone = boneRenderer.bones[j];
                        if (bone.first == null || bone.second == null)
                            continue;

                        DoBoneRender(boneRenderer,bone.first, bone.second, shape, color, size);
                    }

                    for (var k = 0; k < boneRenderer.tips.Length; k++)
                    {
                        var tip = boneRenderer.tips[k];
                        if (tip == null)
                            continue;

                        DoBoneRender(boneRenderer,tip, null, shape, color, size);
                    }
                }

                
            }

            // pyramidMeshRenderer.Render();
            // boxMeshRenderer.Render();

            // Gizmos.color = gizmoColor;
         }


        private static void DoBoneRender(VRBoneRenderer boneRenderer,Transform transform, Transform childTransform, BoneShape shape, Color color, float size)
        {
            Vector3 start = transform.position;
            Vector3 end = childTransform != null ? childTransform.position : start;

            GameObject boneGO = transform.gameObject;

            float length = (end - start).magnitude;
            bool tipBone = (length < k_Epsilon);

            Color highlight = color;
            
            // bool hoveringBone = GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id;
            // hoveringBone = hoveringBone && !SceneVisibilityManager.instance.IsPickingDisabled(transform.gameObject, false);
            bool hoveringBone = false;
            if (hoveringBone)
            {
                // highlight = Handles.preselectionColor;
                highlight = Color.blue;
            }
            // else if (Selection.Contains(boneGO) || Selection.activeObject == boneGO)
            // {
            //     highlight = Handles.selectedColor;
            // }
            
            if (tipBone)
            {
                // Handles.color = highlight;
                // Handles.SphereHandleCap(0, start, Quaternion.identity, k_BoneTipSize * size, EventType.Repaint);
                Gizmos.Sphere(start, k_BoneTipSize * size,highlight);
                // Debug.LogWarning("tipBone");
            }
            else if (shape == BoneShape.Line)
            {
                // Handles.color = highlight;
                // Gizmos.Line(start, end,highlight);
                lineMeshRender.AddInstance(start, end, highlight);
                // Debug.LogWarning("BoneShape.Line");
            }
            else
            {
                // Debug.LogWarning("ComputeBoneMatrix");
                var matrix = ComputeBoneMatrix(start, end, length, size,out var scale,out var direct);
                if (shape == BoneShape.Pyramid)
                {
                    pyramidMeshRenderer.AddInstance(matrix, color, highlight);
                    boneRenderer.UpdateMeshCollider(transform,childTransform, matrix, pyramidMeshRenderer.mesh,pyramidMeshRenderer.collideMesh);
                }
                else
                {
                    // if (shape == BoneShape.Box)
                    boxMeshRenderer.AddInstance(matrix, color, highlight);
                    boneRenderer.UpdateMeshCollider(transform,childTransform, matrix, pyramidMeshRenderer.mesh,boxMeshRenderer.collideMesh);
                }
            }
            
            // int id = GUIUtility.GetControlID(s_ButtonHash, FocusType.Passive);
            // Event evt = Event.current;
            //
            // switch (evt.GetTypeForControl(id))
            // {
            //     case EventType.Layout:
            //         {
            //             HandleUtility.AddControl(id, tipBone ? HandleUtility.DistanceToCircle(start, k_BoneTipSize * size * 0.5f) : HandleUtility.DistanceToLine(start, end));
            //             break;
            //         }
            //     case EventType.MouseMove:
            //         if (id == HandleUtility.nearestControl)
            //             HandleUtility.Repaint();
            //         break;
            //     case EventType.MouseDown:
            //         {
            //             if (evt.alt)
            //                 break;
            //
            //             if (HandleUtility.nearestControl == id && evt.button == 0)
            //             {
            //                 if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
            //                 {
            //                     GUIUtility.hotControl = id; // Grab mouse focus
            //                     EditorHelper.HandleClickSelection(boneGO, evt);
            //                     evt.Use();
            //                 }
            //             }
            //             break;
            //         }
            //     case EventType.MouseDrag:
            //         {
            //             if (!evt.alt && GUIUtility.hotControl == id)
            //             {
            //                 if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
            //                 {
            //                     DragAndDrop.PrepareStartDrag();
            //                     DragAndDrop.objectReferences = new UnityEngine.Object[] {transform};
            //                     DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(transform));
            //
            //                     GUIUtility.hotControl = 0;
            //
            //                     evt.Use();
            //                 }
            //             }
            //             break;
            //         }
            //     case EventType.MouseUp:
            //         {
            //             if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
            //             {
            //                 GUIUtility.hotControl = 0;
            //                 evt.Use();
            //             }
            //             break;
            //         }
            //     case EventType.Repaint:
            //         {
            //             Color highlight = color;
            //
            //             bool hoveringBone = GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id;
            //             hoveringBone = hoveringBone && !SceneVisibilityManager.instance.IsPickingDisabled(transform.gameObject, false);
            //
            //             if (hoveringBone)
            //             {
            //                 highlight = Handles.preselectionColor;
            //             }
            //             else if (Selection.Contains(boneGO) || Selection.activeObject == boneGO)
            //             {
            //                 highlight = Handles.selectedColor;
            //             }
            //
            //             if (tipBone)
            //             {
            //                 Handles.color = highlight;
            //                 Handles.SphereHandleCap(0, start, Quaternion.identity, k_BoneTipSize * size, EventType.Repaint);
            //             }
            //             else if (shape == BoneShape.Line)
            //             {
            //                 Handles.color = highlight;
            //                 Handles.DrawLine(start, end);
            //             }
            //             else
            //             {
            //                 if (shape == BoneShape.Pyramid)
            //                     pyramidMeshRenderer.AddInstance(ComputeBoneMatrix(start, end, length, size), color, highlight);
            //                 else // if (shape == BoneShape.Box)
            //                     boxMeshRenderer.AddInstance(ComputeBoneMatrix(start, end, length, size), color, highlight);
            //             }
            //
            //         }
            //         break;
            // }
            
        }

        public static void OnAddBoneRenderer(VRBoneRenderer obj)
        {
            s_BoneRendererComponents.Add(obj);
        }

        public static void OnRemoveBoneRenderer(VRBoneRenderer obj)
        {
            s_BoneRendererComponents.Remove(obj);
        }
    }
}
