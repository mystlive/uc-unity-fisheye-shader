using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UC.Fisheye
{
    public class CustomPassFisheye : CustomPass
    {
        public enum LensType
        {
            Diagonal, CircularHorizontal, CircularVertical
        }

        public Camera centralVision;
        public Camera peripheralVision;
        public LensType lens = LensType.Diagonal;
        [SerializeField, Range(1.0f, 180.0f)]
        public float fieldOfView = 180;
        [ColorUsage(false, true)]
        public Color32 outsideColor = new(255, 0, 0, 128);
        GraphicsFormat bufferColorFormat = GraphicsFormat.B10G11R11_UFloatPack32;

        // シェーダーが参照するビルドにあることを確認するために、シェーダーへの参照を維持する
        [SerializeField, HideInInspector]
        Shader outlineShader;
        Material material;
        RTHandle bufferTexture;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            outlineShader = Shader.Find("Hidden/CustomPassFisheye");
            material = CoreUtils.CreateEngineMaterial(outlineShader);
            bufferTexture = RTHandles.Alloc(
                Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                colorFormat: bufferColorFormat,
                useDynamicScale: true, name: "Peripheral View Texture"
            );
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var currentCam = ctx.hdCamera.camera;
            if (currentCam == peripheralVision)
            {
                CustomPassUtils.Copy(ctx, ctx.cameraColorBuffer, bufferTexture);
            }
            else if (currentCam == centralVision)
            {
                var aspect = centralVision.aspect;
                var lensCoeff = fieldOfView / 180;
                if (lens == LensType.CircularHorizontal) lensCoeff /= aspect;
                else if (lens == LensType.Diagonal) lensCoeff /= Mathf.Sqrt(aspect * aspect + 1);
                ctx.propertyBlock.SetTexture("_PeripheralTex", bufferTexture);
                ctx.propertyBlock.SetColor("_OutsideColor", outsideColor);
                ctx.propertyBlock.SetFloat("_LensCoeff", lensCoeff);
                ctx.propertyBlock.SetFloat("_InverseCHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * centralVision.fieldOfView));
                ctx.propertyBlock.SetFloat("_InversePHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * peripheralVision.fieldOfView));
                CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.None);
                CoreUtils.DrawFullScreen(ctx.cmd, material, ctx.propertyBlock, shaderPassId: 0);
            }
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(material);
            bufferTexture.Release();
        }
    }
}
