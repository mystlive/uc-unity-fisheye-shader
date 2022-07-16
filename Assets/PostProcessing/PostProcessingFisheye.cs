using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UC.Fisheye
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Fisheye")]
    public sealed class PostProcessingFisheye : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter fieldOfView = new ClampedFloatParameter(180f, 0f, 180f);

        public enum LensType
        {
            Diagonal, CircularHorizontal, CircularVertical
        }
        public LensType lens = LensType.Diagonal;
        [ColorUsage(false, true)]
        public Color32 outsideColor = new(255, 0, 0, 128);

        Material material = null;


        public bool IsActive() => material != null && fieldOfView.value > 0f;

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Settings).
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        //public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        const string kShaderName = "Hidden/PostProcessingFisheye";

        public override void Setup()
        {
            if (Shader.Find(kShaderName) != null)
                material = new Material(Shader.Find(kShaderName));
            else
                Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Fisheye is unable to load.");
        }
        //RTHandle second;
        public static Camera wideCamera;

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            //Debug.Log($"Fisheye@{this.GetHashCode()}.Render({camera}) {fov2.value} wideCamera={wideCamera} wideview{wideCamera?.targetTexture?.GetHashCode()} mat@{material?.GetHashCode()}");
            if (material == null) return;
            var centralVision = camera.camera;
            var aspect = centralVision.aspect;
            var lensCoeff = fieldOfView.value / 180;
            if (lens == LensType.CircularHorizontal) lensCoeff /= aspect;
            else if (lens == LensType.Diagonal) lensCoeff /= Mathf.Sqrt(aspect * aspect + 1);

            //material.SetTexture("_PeripheralTex", bufferTexture);
            material.SetColor("_OutsideColor", outsideColor);
            material.SetFloat("_LensCoeff", lensCoeff);
            material.SetFloat("_InverseCHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * centralVision.fieldOfView));
            //material.SetFloat("_InversePHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * peripheralVision.fieldOfView));

            cmd.Blit(source, destination, material, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(material);
        }
    }
}