using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VidStreamComp
{
    /// <summary>
    /// Acquire images from a video stream coming from a local webcam
    /// </summary>
    [RequireComponent(typeof(WebcamComponent))]
    public class WebcamStream : VideoStream
    {
        private WebcamComponent webCamComponent;

        /// <summary>
        /// The webcam is playing (read only).
        /// </summary>
        public override bool Playing
        {
            get
            {
                return webCamComponent.Playing;
            }
        }

        /// <summary>
        /// The webcam is paused (read only).
        /// </summary>
        public override bool Paused
        {
            get
            {
                return webCamComponent.Paused;
            }
        }

        /// <summary>
        /// The webcam is initialized (read only).
        /// </summary>
        public override bool Initialized
        {
            get
            {
                return webCamComponent.Initialized;
            }
        }

        /// <summary>
        /// The name of the webcam device (read only).
        /// </summary>
        public string DeviceName
        {
            get
            {
                return webCamComponent.DeviceName;
            }
            set
            {
                if (webCamComponent.DeviceName != value)
                {
                    bool init = Initialized;
                    bool playing = Playing;
                    if (init)
                    {
                        Terminate();
                    }
                    webCamComponent.DeviceName = value;
                    if (init)
                    {
                        Initialize();
                    }
                    if (playing)
                    {
                        Play();
                    }
                }
            }
        }

        public override int FrameWidth
        {
            get
            {
                return webCamComponent.FrameWidth;
            }
        }

        public override int FrameHeight
        {
            get
            {
                return webCamComponent.FrameHeight;
            }
        }

        public override Texture FrameTexture
        {
            get
            {
                return webCamComponent.FrameTexture;
            }
        }

        protected override bool FrameTextureChanged
        {
            get
            {
                return webCamComponent.FrameTextureUpdated;           
            }
        }

        protected override void PlayVideo()
        {
            webCamComponent.Play();
        }

        protected override void PauseVideo()
        {
            webCamComponent.Pause();
        }

        protected override void StopVideo()
        {
            webCamComponent.Stop();
        }

        protected override bool InitializeVideo()
        {
            return webCamComponent.Initialize();
        }

        protected override void TerminateVideo()
        {
            webCamComponent.Terminate();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            webCamComponent.TerminateEvent += WebCamComponent_TerminateEvent;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            webCamComponent.TerminateEvent -= WebCamComponent_TerminateEvent;
        }

        private void WebCamComponent_TerminateEvent()
        {
            OnTerminate();
        }

        private void Awake()
        {
            webCamComponent = GetComponent<WebcamComponent>();
        }
    }
}
