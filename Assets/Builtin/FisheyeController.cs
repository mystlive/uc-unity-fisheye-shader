using System;
using UnityEngine;

namespace UC.Fisheye
{
    [Serializable]
    public class FisheyeController : MonoBehaviour
    {
        [SerializeField]
        public Material material;
        [SerializeField]
        public Camera targetCamera;
        public enum LensType
        {
            Diagonal, Circular, CircularHorizontal, CircularVertical
        }

        public LensType lens = LensType.Diagonal;
        [ColorUsage(false, true)]
        public Color32 outsideColor = new(255, 0, 0, 128);

        [SerializeField, Range(1.0f, 180.0f)]
        public float fieldOfView = 180;
        [SerializeField, Range(1.0f, 179.0f)]
        public float centralFieldOfView = 80;
        [SerializeField, Range(1.0f, 179.0f)]
        public float peripheralFieldOfView = 170;
        [SerializeField]
        public Vector2Int renderTextureSize = new(2048, 2048);
        [SerializeField, Range(0, 10)]
        public int renderTextureScale = 0;
        [SerializeField, Range(0, 32)]
        public int renderTexturewDepth = 16;
        [SerializeField, Range(1, 30)]
        public int frameSkipInterval = 0;

        private RenderTexture bufferTexture;


        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
            targetCamera.fieldOfView = centralFieldOfView;
        }
        void OnPreCull()
        {
            if (Time.frameCount % frameSkipInterval == 0)
            {
                if (targetCamera == null || material == null) return;
                if (renderTextureScale > 0)
                {
                    renderTextureSize.Set(targetCamera.pixelWidth, targetCamera.pixelHeight);
                    renderTextureSize *= renderTextureScale;
                }
                bufferTexture = bufferTexture.Reallocate(renderTextureSize.x, renderTextureSize.y, renderTexturewDepth);
                var t = targetCamera.targetTexture;
                var f = targetCamera.fieldOfView;

                targetCamera.aspect = targetCamera.aspect;
                targetCamera.targetTexture = bufferTexture;
                targetCamera.fieldOfView = peripheralFieldOfView;
                this.enabled = false;
                targetCamera.Render();
                this.enabled = true;
                targetCamera.targetTexture = t;
                targetCamera.fieldOfView = centralFieldOfView;
                targetCamera.ResetAspect();
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var aspect = targetCamera.aspect;
            var lensCoeff = fieldOfView / 180;
            if (lens == LensType.Circular) lensCoeff /= Mathf.Min(aspect, 1);
            else if (lens == LensType.CircularHorizontal) lensCoeff /= aspect;
            else if (lens == LensType.Diagonal) lensCoeff /= Mathf.Sqrt(aspect * aspect + 1);

            material.SetTexture("_PeripheralTex", bufferTexture);
            material.SetColor("_OutsideColor", outsideColor);
            material.SetFloat("_LensCoeff", lensCoeff);
            material.SetFloat("_InverseCHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * targetCamera.fieldOfView));
            material.SetFloat("_InversePHeightHalf", 1.0f / Mathf.Tan(0.5f * Mathf.Deg2Rad * peripheralFieldOfView));

            Graphics.Blit(source, destination, material);
        }
    }
}