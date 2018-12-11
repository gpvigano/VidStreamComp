using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VidStreamComp
{
    /// <summary>
    /// Abstract class for rendering video to a material.
    /// </summary>
    public abstract class VideoStreamToMaterial : MonoBehaviour
    {
        /// <summary>
        /// Source video stream.
        /// </summary>
        [Tooltip("Source video stream")]
        [SerializeField]
        protected VideoStream videoStream;

        /// <summary>
        /// Directly change materials instead of using their copies.
        /// </summary>
        [Tooltip("Directly change materials instead of using their copies")]
        [SerializeField]
        protected bool sharedMaterial = false;

        /// <summary>
        /// Initialize the material duplicating the original one if sharedMaterial is false.
        /// </summary>
        protected abstract void InitializeMaterial();

        /// <summary>
        /// Update the frame texture.
        /// </summary>
        public abstract void UpdateFrameTexture(Texture frameTexture);

        /// <summary>
        /// Update the frame size.
        /// </summary>
        public abstract void UpdateFrameSize(int frameWidth, int frameHeight);

        /// <summary>
        /// Reset the material texture.
        /// </summary>
        protected abstract void ResetMaterial();

        /// <summary>
        /// Update the material texture and size according to the current video frame.
        /// </summary>
        protected virtual void UpdateFrame()
        {
            UpdateFrameTexture(videoStream.FrameTexture);
            UpdateFrameSize(videoStream.FrameWidth, videoStream.FrameHeight);
        }

        /// <summary>
        /// On enable register to events.
        /// </summary>
        protected virtual void OnEnable()
        {
            videoStream.FrameUpdateEvent += UpdateFrame;
            videoStream.InitializedEvent += InitializeMaterial;
            videoStream.TerminateEvent += ResetMaterial;
        }

        /// <summary>
        /// On disable unregister to events.
        /// </summary>
        protected virtual void OnDisable()
        {
            videoStream.FrameUpdateEvent -= UpdateFrame;
            videoStream.InitializedEvent -= InitializeMaterial;
            videoStream.TerminateEvent -= ResetMaterial;
        }
    }
}
