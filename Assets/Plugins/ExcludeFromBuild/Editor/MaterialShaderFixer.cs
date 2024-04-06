using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.ExcludeFromBuild
{
    public static class MaterialShaderFixer
    {
        public enum RenderPiplelineType
        {
            URP, HDRP, Standard
        }

        static Dictionary<string, Color> Materials = new Dictionary<string, Color> {
            { "Assets/ExcludeFromBuild/Examples/Materials/Sky.mat", Color.white },
            { "Assets/ExcludeFromBuild/Examples/Materials/TreeLeavesGreen.mat" , new Color(0.15f, 0.6f, 0.2f) },
            { "Assets/ExcludeFromBuild/Examples/Materials/TreeTrunk.mat", new Color(0.547f, 0.25f, 0f) },
        };

        public static void FixMaterials(RenderPiplelineType createdForRenderPipleline)
        {
            var currentRenderPipline = GetCurrentRenderPiplelineType();

            if (currentRenderPipline != createdForRenderPipleline)
            {
                Shader shader = getDefaultShader();
                foreach (var kv in Materials)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(kv.Key);
                    if (material == null)
                        Debug.LogWarning("Could not upgrade shader for: " + kv.Key);
                    material.shader = shader;
                    material.color = kv.Value;
                    EditorUtility.SetDirty(material);
                }
            }

            AssetDatabase.SaveAssets();
        }

        public static RenderPiplelineType GetCurrentRenderPiplelineType()
        {
            // Assume URP as default
            var renderPipeline = RenderPiplelineType.URP;

            // check if Standard or HDRP
            if (getUsedRenderPipeline() == null)
                renderPipeline = RenderPiplelineType.Standard; // Standard
            else if (!getUsedRenderPipeline().GetType().Name.Contains("Universal"))
                renderPipeline = RenderPiplelineType.HDRP; // HDRP

            return renderPipeline;
        }

        static Shader getDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        /// <summary>
        /// Returns the current pipline. Returns NULL if it's the standard render pipeline.
        /// </summary>
        /// <returns></returns>
        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

    }
}