using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;

namespace Content.Scritps.Charactor
{
    
    public class BatchRenderer
    {
        const int kMaxDrawMeshInstanceCount = 1023;

        public enum SubMeshType
        {
            BoneFaces,
            BoneWire,
            Count
        }

        public Mesh mesh;
        public Mesh collideMesh;
        public Material material;
        public Material material2;

        private List<Matrix4x4> m_Matrices = new List<Matrix4x4>();
        private List<Vector4> m_Colors = new List<Vector4>();
        private List<Vector4> m_Highlights = new List<Vector4>();

        public void AddInstance(Matrix4x4 matrix, Color color, Color highlight)
        {
            m_Matrices.Add(matrix);
            m_Colors.Add(color);
            m_Highlights.Add(highlight);
        }

        public void Clear()
        {
            Debug.LogWarning("Clear Batch Render");
            m_Matrices.Clear();
            m_Colors.Clear();
            m_Highlights.Clear();
        }

        private static int RenderChunkCount(int totalCount)
        {
            return Mathf.CeilToInt((totalCount / (float)kMaxDrawMeshInstanceCount));
        }

        private static T[] GetRenderChunk<T>(List<T> array, int chunkIndex)
        {
            int rangeCount = (chunkIndex < (RenderChunkCount(array.Count) - 1)) ?
                kMaxDrawMeshInstanceCount : array.Count - (chunkIndex * kMaxDrawMeshInstanceCount);

            return array.GetRange(chunkIndex * kMaxDrawMeshInstanceCount, rangeCount).ToArray();
        }

        public void Render(ScriptableRenderContext context)
        {
            if (m_Matrices.Count == 0 || m_Colors.Count == 0 || m_Highlights.Count == 0)
                return;
            

            int count = System.Math.Min(m_Matrices.Count, System.Math.Min(m_Colors.Count, m_Highlights.Count));

            // Material mat = material;
            if (material2 == null)
            {
                material2 = Object.Instantiate(material);
            }
            material.SetPass(0);
            material2.SetPass(0);

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            CommandBuffer cb = new CommandBuffer();
            cb.name = this.mesh.name;
            var m_ProfilingSampler = new ProfilingSampler(cb.name+"Profile");
            using (new ProfilingScope(cb, m_ProfilingSampler))
            {
                Matrix4x4[] matrices = null;
                // Debug.LogWarning("Do Render BatchRender :" + count);
                int chunkCount = RenderChunkCount(count);
                for (int i = 0; i < chunkCount; ++i)
                {
                    matrices = GetRenderChunk(m_Matrices, i);
                    // Debug.LogWarning("Render BatchRender Index:" + i+" matrices count:"+matrices.Length);
                
                    cb.Clear();
                    propertyBlock.SetVectorArray("_Color", GetRenderChunk(m_Colors, i));
                
                    material.DisableKeyword("WIRE_ON");
                    cb.DrawMeshInstanced(mesh, (int) SubMeshType.BoneFaces, material, 0, matrices, matrices.Length,
                        propertyBlock);
                    // Graphics.ExecuteCommandBuffer(cb);
                    context.ExecuteCommandBuffer(cb);
                
                    cb.Clear();
                    propertyBlock.SetVectorArray("_Color", GetRenderChunk(m_Highlights, i));
                
                    material2.EnableKeyword("WIRE_ON");
                    cb.DrawMeshInstanced(mesh, (int) SubMeshType.BoneWire, material2, 0, matrices, matrices.Length,
                        propertyBlock);
                    // Graphics.ExecuteCommandBuffer(cb);
                    context.ExecuteCommandBuffer(cb);
                }
            }
        }

        
    }
}