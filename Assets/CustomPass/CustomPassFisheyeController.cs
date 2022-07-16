using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

namespace UC.Fisheye
{
    public class CustomPassFisheyeController : MonoBehaviour
    {
        public Camera targetCamera;

        public CustomPassFisheye.LensType lens;
        [SerializeField, Range(1.0f, 180.0f)]
        public float fieldOfView = 180;
        [SerializeField, Range(1.0f, 179.0f)]
        public float peripheralFieldOfView = 170;

        [SerializeField]
        public Vector2Int renderTextureSize = new(2048, 2048);
        [SerializeField, Range(0, 10)]
        public int renderTextureScale = 0;
        public GraphicsFormat renderTextureColorFormat = GraphicsFormat.R8G8B8A8_SNorm;
        public GraphicsFormat renderTextureDepthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
        public RenderTexture renderTexture;


        private CustomPassFisheye customPass;
        private Camera peripheralVision;

        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
            peripheralVision = targetCamera.gameObject.CreateChild<Camera>("Peripheral Vision Camera");
            
            var customPassVolume = gameObject.AddComponent<CustomPassVolume>();
            customPass = (CustomPassFisheye)customPassVolume.AddPassOfType<CustomPassFisheye>();
            customPass.name = "Fisheye";

            customPass.centralVision = targetCamera;
            customPass.peripheralVision = peripheralVision;
        }
        void LateUpdate()
        {
            //if (targetCamera == null || material == null) return;
            if (renderTextureScale > 0)
            {
                renderTextureSize.Set(targetCamera.pixelWidth, targetCamera.pixelHeight);
                renderTextureSize *= renderTextureScale;
            }
            renderTexture = renderTexture.Reallocate(renderTextureSize.x, renderTextureSize.y, renderTextureColorFormat, renderTextureDepthStencilFormat);
            peripheralVision.CopyFrom(targetCamera);
            peripheralVision.fieldOfView = peripheralFieldOfView;
            peripheralVision.depth = targetCamera.depth - 1;
            peripheralVision.aspect = targetCamera.aspect;
            peripheralVision.targetTexture = renderTexture;
            customPass.fieldOfView = fieldOfView;
            customPass.lens = lens;
        }
    }
}