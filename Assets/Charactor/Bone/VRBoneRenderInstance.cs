// using System;
// using UnityEngine;
// using UnityEngine.Rendering;
//
// namespace Content.Scritps
// {
//     public class VRBoneRenderInstance:MonoBehaviour
//     {
//         public void OnEnable()
//         {
//             if (GraphicsSettings.renderPipelineAsset == null)
//             {
//                 Camera.onPostRender += OnRendered;
//             }
//             else
//             {
//                 RenderPipelineManager.endCameraRendering += OnRendered;
//             }
//         }
//
//
//         private void OnDisable()
//         {
//             if (GraphicsSettings.renderPipelineAsset == null)
//             {
//                 Camera.onPostRender -= OnRendered;
//             }
//             else
//             {
//                 RenderPipelineManager.endCameraRendering -= OnRendered;
//             }
//         }
//
//         private void OnRendered(Camera cam)
//         {
//             
//         }
//
//         private void OnRendered(ScriptableRenderContext arg1, Camera arg2)
//         {
//             VRBoneRendererUtils.pyramidMeshRenderer.Render(arg1);
//             VRBoneRendererUtils.boxMeshRenderer.Render(arg1);
//         }
//     }
// }