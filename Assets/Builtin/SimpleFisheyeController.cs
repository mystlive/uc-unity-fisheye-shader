using UnityEngine;

public class SimpleFisheyeController : MonoBehaviour
{
    public Material material;
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_CameraFov", targetCamera.fieldOfView);
        Graphics.Blit(source, destination, material);
    }
}
