using UnityEngine;

namespace UC.Fisheye
{
    public class CubemapFisheyeController : MonoBehaviour
    {
        public Material material;
        public Camera targetCamera;
        public RenderTexture cubemap;
        public int cubeMapSize = 1024;
        private bool flag = false;

        void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }
        void LateUpdate()
        {
            if (targetCamera == null || material == null) return;

            if (cubemap == null || cubemap.width != cubeMapSize)
            {
                if (cubemap != null)
                    Destroy(cubemap);

                cubemap = new RenderTexture(cubeMapSize, cubeMapSize, 24, RenderTextureFormat.ARGB32);
                cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
                cubemap.Create();
            }
            flag = true;
            targetCamera.RenderToCubemap(cubemap);
            flag = false;
        }
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (flag)
            {
                Graphics.Blit(source, destination);
            }
            else
            {
                Quaternion rot = Quaternion.Inverse(targetCamera.transform.rotation);
                material.SetVector("_Rotation", new Vector4(rot.x, rot.y, rot.z, rot.w));
                Graphics.Blit(cubemap, destination, material);
            }
        }
    }
}