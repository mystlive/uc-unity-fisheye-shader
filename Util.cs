using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.Experimental.Rendering;

namespace UC
{
    public static partial class Util
    {
        public static RenderTexture Reallocate(this RenderTexture old, int width, int height, int depth)
        {
            if ((width <= 0) || (height <= 0) || (depth <= 0))
            {
                if (old)
                {
                    old.DiscardContents(false, false);
                    old.Release();
                }
                return null;
            }
            else if (!old)
            {
                return new RenderTexture(width, height, depth);
            }
            else if ((old.width != width) || (old.height != height) || (old.depth != depth))
            {
                Debug.Log($"{old.width}x{old.height}x{old.depth} -> {width}x{height}x{depth}");
                old.DiscardContents(false, false);
                old.Release();
                return new RenderTexture(width, height, depth);
            }
            else
            {
                return old;
            }
        }
        public static RenderTexture Reallocate(this RenderTexture old, int width, int height, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SNorm, GraphicsFormat depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt)
        {
            if ((width <= 0) || (height <= 0))
            {
                if (old)
                {
                    old.DiscardContents(false, false);
                    old.Release();
                }
                return null;
            }
            else if (!old)
            {
                return new RenderTexture(width, height, colorFormat, depthStencilFormat);
            }
            else if ((old.width != width) || (old.height != height) || (old.graphicsFormat != colorFormat) || (old.depthStencilFormat != depthStencilFormat))
            {
                Debug.Log($"{old.width}x{old.height}/{old.graphicsFormat}/{old.depthStencilFormat} -> {width}x{height}/{colorFormat}/{depthStencilFormat}");
                old.DiscardContents(false, false);
                old.Release();
                return new RenderTexture(width, height, colorFormat, depthStencilFormat);
            }
            else
            {
                return old;
            }
        }


        public static GameObject CreateGameObject(string name, Transform parent = null, Mesh mesh = null, Material material = null)
        {
            var obj = new GameObject(name);
            if (parent != null)
            {
                var objT = obj.transform;
                objT.SetParent(parent, false);
                obj.layer = parent.gameObject.layer;
            }
            if (material != null)
            {
                var renderer = obj.AddComponent<MeshRenderer>();
                renderer.material = material;
            }
            if (mesh != null)
            {
                var filter = obj.AddComponent<MeshFilter>();
                filter.mesh = mesh;
            }
            return obj;
        }
        public static GameObject CreateChild(this GameObject parent, string name, Mesh mesh = null, Material material = null)
        {
            return CreateGameObject(name, parent.transform, mesh, material);
        }
        public static T CreateChild<T>(this GameObject parent, string name, Mesh mesh = null, Material material = null) where T : Component
        {
            return parent.CreateChild(name, mesh, material).AddComponent<T>();
        }
    }
}