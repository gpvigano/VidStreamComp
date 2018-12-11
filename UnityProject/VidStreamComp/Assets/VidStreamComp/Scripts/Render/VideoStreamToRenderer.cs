using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// Video rendering to a material of a Renderer.
    /// </summary>
    public class VideoStreamToRenderer : VideoStreamToMaterial
    {
        /// <summary>
        /// Renderer with the material texture for the frames.
        /// </summary>
        [Tooltip("Renderer with the material texture for the frames")]
        [SerializeField]
        private Renderer frameRenderer;

        private Material rendererMaterial = null;

        public override void UpdateFrameSize(int frameWidth, int frameHeight)
        {
            // TODO: keep aspect if texture is cropped
            Texture frameTexture = videoStream.FrameTexture;
            if (rendererMaterial != null && frameTexture != null)
            {
                rendererMaterial.mainTexture = frameTexture;
            }
        }

        public override void UpdateFrameTexture(Texture frameTexture)
        {
            if (rendererMaterial != null && rendererMaterial.mainTexture != frameTexture)
            {
                rendererMaterial.mainTexture = frameTexture;
            }
        }

        /// <summary>
        /// Initialize the materials duplicating the original ones if sharedMaterials is false.
        /// </summary>
        protected override void InitializeMaterial()
        {
            if (frameRenderer != null && rendererMaterial == null)
            {
                if (sharedMaterial && frameRenderer.sharedMaterial != null)
                {
                    rendererMaterial = frameRenderer.sharedMaterial;
                }
                else
                {
                    rendererMaterial = new Material(frameRenderer.material);
                    frameRenderer.sharedMaterial = rendererMaterial;
                }
                rendererMaterial.mainTexture = videoStream.FrameTexture;
            }
        }

        protected override void ResetMaterial()
        {
            if (rendererMaterial != null)
            {
                rendererMaterial.mainTexture = null;
            }
        }
    }
}
