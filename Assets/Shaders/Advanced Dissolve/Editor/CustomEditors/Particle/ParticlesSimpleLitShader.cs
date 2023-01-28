using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Rendering.LWRP.ShaderGUI
{
    internal class AdvancedDisolve_ParticlesSimpleLitShader : BaseShaderGUI
    {
        // Properties
        private UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties shadingModelProperties;
        private UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.ParticleProperties particleProps;
        
        // List of renderers using this material in the scene, used for validating vertex streams
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();
        
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties(properties);
            particleProps = new UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.ParticleProperties(properties);


             //VacuumShaders
            VacuumShaders.AdvancedDissolve.MaterialProperties.Init(properties);
        }
        
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SetMaterialKeywords, UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.SetMaterialKeywords);


            //VacuumShaders
            VacuumShaders.AdvancedDissolve.MaterialProperties.MaterialChanged(material);
        }
        
        public override void DrawSurfaceOptions(Material material)
        {
            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                base.DrawSurfaceOptions(material);
                DoPopup(UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.Styles.colorMode, particleProps.colorMode, Enum.GetNames(typeof(UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.ColorMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
        }
        
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Inputs(shadingModelProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
        }
        
        public override void DrawAdvancedOptions(Material material)
        {
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Advanced(shadingModelProperties);
            EditorGUI.BeginChangeCheck();
            {
                materialEditor.ShaderProperty(particleProps.flipbookMode, UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.Styles.flipbookMode);
                UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.FadingOptions(material, materialEditor, particleProps);
                UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.DoVertexStreamsArea(material, m_RenderersUsingThisMaterial, true);
            }
            base.DrawAdvancedOptions(material);
        }

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            CacheRenderersUsingThisMaterial(material);
            base.OnOpenGUI(material, materialEditor);
        }
        
        void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }


        public override void DrawAdditionalFoldouts(Material material)
        {
            //VacuumShaders
            VacuumShaders.AdvancedDissolve.MaterialProperties.DrawDissolveOptions(materialEditor, false, false);
        }
    }
} // namespace UnityEditor
