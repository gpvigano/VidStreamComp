using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// Video rendering to a material of an Image.
    /// </summary>
    [RequireComponent(typeof(FrameImageResizer))]
    public class VideoStreamToImage : VideoStreamToMaterial
    {
        /// <summary>
        /// Show the image game object when playing (or paused) and hide it when stopped.
        /// </summary>
        [Tooltip("Show the image game object when playing (or paused) and hide it when stopped")]
        [SerializeField]
        protected bool toggleImage = true;

        /// <summary>
        /// Image with the material texture for the frames.
        /// </summary>
        [Tooltip("Image with the material texture for the frames")]
        [SerializeField]
        private Image frameImage;

        /// <summary>
        /// Image resizer for frameImage.
        /// </summary>
        [Tooltip("Image resizer for the above image")]
        [SerializeField]
        private FrameImageResizer imageResizer;

        private Material imageMaterial = null;

        public override void UpdateFrameSize(int frameWidth, int frameHeight)
        {
            if (imageResizer != null)
            {
                imageResizer.UpdateFrameSize(frameWidth, frameHeight);
            }
        }

        public override void UpdateFrameTexture(Texture frameTexture)
        {
            if (imageMaterial != null && imageMaterial.mainTexture != frameTexture)
            {
                imageMaterial.mainTexture = frameTexture;
            }
        }

        /// <summary>
        /// Get a FrameImageResizer and set its frame image if defined.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (frameImage != null)
            {
                if (imageResizer == null)
                {
                    imageResizer = GetComponent<FrameImageResizer>();
                }
                if (imageResizer != null)
                {
                    imageResizer.SetFrameImage(frameImage);
                }
            }
        }

        protected override void InitializeMaterial()
        {
            if (frameImage != null && imageMaterial == null)
            {
                if (sharedMaterial && frameImage.material != null)
                {
                    imageMaterial = frameImage.material;
                }
                else
                {
                    imageMaterial = new Material(frameImage.material);
                }
                imageMaterial.mainTexture = videoStream.FrameTexture;
            }
            frameImage.material = imageMaterial;
        }

        protected override void ResetMaterial()
        {
            if (imageMaterial != null)
            {
                imageMaterial.mainTexture = null;
                frameImage.material = null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            videoStream.StopEvent += OnStop;
            videoStream.PlayEvent += OnPlay;
            videoStream.TerminateEvent += ResetMaterial;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            videoStream.StopEvent -= OnStop;
            videoStream.PlayEvent -= OnPlay;
            videoStream.TerminateEvent -= ResetMaterial;
        }

        private void OnPlay()
        {
            if (imageResizer != null)
            {
                imageResizer.SwitchResizing(true);
            }
            if (frameImage != null)
            {
                // show the frame image in case it was hidden
                frameImage.gameObject.SetActive(true);
            }
        }

        private void OnStop()
        {
            if (imageResizer != null)
            {
                imageResizer.SwitchResizing(false);
            }
            if (toggleImage && frameImage != null)
            {
                frameImage.gameObject.SetActive(false);
            }
        }
    }
}
