using System;
using UnityEngine;

namespace VidStreamComp
{
    /// <summary>
    /// Webcam behaviour wrapping UnityEngine.WebCamTexture.
    /// </summary>
    public class WebcamComponent : MonoBehaviour
    {
        [Tooltip("Width of the webcam texture")]
        [SerializeField]
        private int frameTextureWidth = 640;
        [Tooltip("Height of the webcam texture")]
        [SerializeField]
        private int frameTextureHeight = 480;
        [Tooltip("Requested frames per second (0 = default)")]
        [SerializeField]
        private float requestedFPS = 0;
        [Tooltip("Name of the device (choose the first available if empty)")]
        [SerializeField]
        private string deviceName = null;
        [Tooltip("Requested quality for extracting JPEG data")]
        [SerializeField]
        [Range(0, 100)]
        private int jpegQuality = 80;

        private WebCamTexture webCamTexture = null;
        private byte[] jpegImageBuffer = null;
        private Texture2D frameTexture = null;

        /// <summary>
        /// Webcam initialized and playing.
        /// </summary>
        public bool Playing
        {
            get
            {
                return Initialized && webCamTexture.isPlaying;
            }
        }

        /// <summary>
        /// Webcam initialized and paused.
        /// </summary>
        public bool Paused
        {
            get
            {
                return Initialized && !webCamTexture.isPlaying;
            }
        }

        /// <summary>
        /// Webcam initialized.
        /// </summary>
        public bool Initialized
        {
            get
            {
                return webCamTexture != null;
            }
        }

        /// <summary>
        /// Webcam device name, updated after initialization.
        /// </summary>
        public string DeviceName
        {
            get
            {
                return deviceName;
            }
            set
            {
                if (deviceName != value)
                {
                    bool init = Initialized;
                    bool playing = Playing;
                    if (init)
                    {
                        Terminate();
                    }
                    deviceName = value;
                    if (init)
                    {
                        Initialize();
                    }
                    if (playing && Initialized)
                    {
                        Play();
                    }
                }
            }
        }

        /// <summary>
        /// Webcam video frame width.
        /// </summary>
        public int FrameWidth
        {
            get
            {
                return Initialized ? webCamTexture.width : 0;
            }
        }

        /// <summary>
        /// Webcam video frame height.
        /// </summary>
        public int FrameHeight
        {
            get
            {
                return Initialized ? webCamTexture.height : 0;
            }
        }

        /// <summary>
        /// Webcam video frame texture.
        /// </summary>
        public Texture FrameTexture
        {
            get
            {
                return webCamTexture;
            }
        }

        /// <summary>
        /// Webcam video frame was updated.
        /// </summary>
        public bool FrameTextureUpdated
        {
            get
            {
                return webCamTexture != null && webCamTexture.didUpdateThisFrame;
            }
        }

        /// <summary>
        /// Webcam video frame rate (frames per second), updated after initialization.
        /// </summary>
        public float RequestedFPS
        {
            get
            {
                return Initialized ? webCamTexture.requestedFPS : this.requestedFPS;
            }

            set
            {
                requestedFPS = value;
                if (Initialized)
                {
                    webCamTexture.requestedFPS = this.requestedFPS;
                }
            }
        }

        public int FrameCounter { get; private set; }

        /// <summary>
        /// Event triggered when setting a new device.
        /// </summary>
        public event Action TerminateEvent;

        /// <summary>
        /// Play the video from the webcam (if initialized).
        /// </summary>
        public void Play()
        {
            if (webCamTexture != null && !webCamTexture.isPlaying)
            {
                webCamTexture.Play();
            }
        }

        /// <summary>
        /// Pause the video from the webcam (if initialized).
        /// </summary>
        public void Pause()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Pause();
            }
        }

        /// <summary>
        /// Stop the video from the webcam (if initialized).
        /// </summary>
        public void Stop()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
        }

        /// <summary>
        /// Initialize the webcam.
        /// </summary>
        /// <returns>Return true if the webcam was successfully initialized, false otherwise.</returns>
        public bool Initialize()
        {
            if (Initialized)
            {
                return true;
            }
            FrameCounter = 0;
            webCamTexture = NewWebCamTexture(frameTextureWidth, frameTextureHeight, deviceName);
            if (webCamTexture != null)
            {
                webCamTexture.requestedFPS = requestedFPS;
                deviceName = webCamTexture.deviceName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set the webcam texture to a given already configured object.
        /// </summary>
        /// <remarks>This method must be called before (in place of) initialization.</remarks>
        /// <returns>Return true if the webcam was successfully set, false otherwise.</returns>
        public bool SetWebcamTexture(WebCamTexture webCam)
        {
            if (Initialized)
            {
                return false;
            }
            FrameCounter = 0;
            webCamTexture = webCam;
            if (webCamTexture != null)
            {
                requestedFPS = webCamTexture.requestedFPS;
                webCamTexture.deviceName = deviceName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Terminate the webcam.
        /// </summary>
        public void Terminate()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
            webCamTexture = null;
            if (TerminateEvent != null)
            {
                TerminateEvent();
            }
        }

        /// <summary>
        /// Convert the current video frame to a JPEG image, only if updated.
        /// </summary>
        /// <returns>Return the converted JPEG image bytes or the cached buffer.</returns>
        public byte[] GetJPG()
        {
            if (jpegImageBuffer == null || webCamTexture.didUpdateThisFrame)
            {
                if (frameTexture == null)
                {
                    frameTexture = new Texture2D(webCamTexture.width, webCamTexture.height);
                }
                else
                {
                    frameTexture.Resize(webCamTexture.width, webCamTexture.height);
                }
                frameTexture.SetPixels32(webCamTexture.GetPixels32());
                jpegImageBuffer = frameTexture.EncodeToJPG(jpegQuality);
            }
            return jpegImageBuffer;
        }

        private WebCamTexture NewWebCamTexture(int frameTextureWidth, int frameTextureHeight, string deviceName)
        {
            WebCamTexture webCamTexture = null;
            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogWarning("No webcam available.");
            }
            else
            {
                if (string.IsNullOrEmpty(deviceName))
                {
                    webCamTexture = new WebCamTexture(frameTextureWidth, frameTextureHeight);
                }
                else
                {
                    webCamTexture = new WebCamTexture(deviceName, frameTextureWidth, frameTextureHeight);
                }
                deviceName = webCamTexture.deviceName;
                Debug.Log("Webcam device: " + deviceName);
                return webCamTexture;
            }
            return null;
        }

        private void Update()
        {
            if (Initialized)
            {
                if (webCamTexture.didUpdateThisFrame)
                {
                    ++FrameCounter;
                }
            }
        }
    }
}
