using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// User interface manager for connecting to an IP camera.
    /// </summary>
    [RequireComponent(typeof(IPCameraStream))]
    public class IPCameraStreamUI : VideoStreamUI
    {
        [Tooltip("Input field for the IP camera URL (MJPEG video, not the HTTP page)")]
        [SerializeField]
        protected InputField urlInputField;

        [Tooltip("Input field for an optional login, if required to access the IP camera")]
        [SerializeField]
        protected InputField loginInputField;

        [Tooltip("Input field for an optional password, if required to access the IP camera")]
        [SerializeField]
        protected InputField passwordInputField;

        [Tooltip("Script that controls the IP camera rendering")]
        [SerializeField]
        protected IPCameraStream ipCameraStream;

        [Tooltip("Color for the URL input field when the connection fails.")]
        [SerializeField]
        protected Color connectionFailedColor = Color.red;

        [Tooltip("Color for the URL input field when the connection succeeds.")]
        [SerializeField]
        protected Color connectionSucceededColor = Color.green;

        [Tooltip("Default IP camera configuration")]
        [SerializeField]
        protected VideoStreamingSettings defaultSettings = new VideoStreamingSettings();

        [Tooltip("Start streaming on startup")]
        [SerializeField]
        private bool autoStartStreaming = false;

        protected Color connectionDefaultColor = Color.black;

       /// <summary>
        /// IP camera video streming settings.
        /// </summary>
        public virtual VideoStreamingSettings VideoStreamSettings
        {
            get
            {
                return defaultSettings;
            }
        }

        /// <summary>
        /// Change the video streaming setting according to the UI content and restart video streaming.
        /// </summary>
        public virtual void ApplyUIChangesAndStartVideo()
        {
            UpdateVideoStreamSettings();
            RestartVideo();
        }

        /// <summary>
        /// Update the internal video streming setting according to the UI content
        /// </summary>
        public virtual void UpdateVideoStreamSettings()
        {
            if (urlInputField != null)
            {
                VideoStreamSettings.videoUrl = urlInputField.text;
            }
            if (loginInputField != null)
            {
                VideoStreamSettings.login = loginInputField.text;
            }
            if (passwordInputField != null)
            {
                VideoStreamSettings.password = passwordInputField.text;
            }
        }

        /// <summary>
        /// Restart video streaming using the current settings
        /// </summary>
        public virtual void RestartVideo()
        {
            StopVideo();
            SetUrl();
            StartVideo();
        }

        /// <summary>
        /// Stop the video streaming, if started
        /// </summary>
        public virtual void StopVideo()
        {
            ipCameraStream.Stop();
            ToggleButtons(false);
        }

        /// <summary>
        /// Start video streaming from the IP camera with last settings, restart if already running.
        /// </summary>
        public virtual void StartVideo()
        {
            ipCameraStream.Play();
        }

        /// <summary>
        /// Update the user interface according to the current Video Stream Settings.
        /// </summary>
        /// <param name="videoStreamSettings"></param>
        protected virtual void UpdateUI(VideoStreamingSettings videoStreamSettings)
        {
            if (urlInputField != null)
            {
                urlInputField.text = videoStreamSettings.videoUrl;
            }
            if (loginInputField != null)
            {
                loginInputField.text = videoStreamSettings.login;
            }
            if (passwordInputField != null)
            {
                passwordInputField.text = videoStreamSettings.password;
            }
        }

        /// <summary>
        /// Called when the connection to the IP camera succeeds.
        /// </summary>
        protected virtual void OnConnectionSucceeded()
        {
            if (urlInputField != null)
            {
                urlInputField.textComponent.color = connectionSucceededColor;
            }

            ToggleButtons(true);
        }

        /// <summary>
        /// Called when the connection to the IP camera fails.
        /// </summary>
        protected virtual void OnConnectionFailed()
        {
            if (urlInputField != null)
            {
                urlInputField.textComponent.color = connectionFailedColor;
            }
            ToggleButtons(false);
        }

        /// <summary>
        /// Called when the connection to the IP camera is closed.
        /// </summary>
        private void OnConnectionClosed()
        {
            if (urlInputField != null)
            {
                urlInputField.textComponent.color = connectionDefaultColor;
            }
            ToggleButtons(false);
        }

        /// <summary>
        /// Set URL and optionally login and password for the IP camera according to the current Video Stream Settings.
        /// </summary>
        protected virtual void SetUrl()
        {
            string url = VideoStreamSettings.videoUrl;
            string login = VideoStreamSettings.login;
            string password = VideoStreamSettings.password;
            if (string.IsNullOrEmpty(url))
            {
                url = ipCameraStream.Url;
                login = ipCameraStream.Login;
                password = ipCameraStream.Password;
            }
            else
            {
                ipCameraStream.SetUrl(url, login, password);
            }
        }

        /// <summary>
        /// Initialize default settings
        /// </summary>
        protected virtual void Awake()
        {
            ipCameraStream = GetComponent<IPCameraStream>();
            if (urlInputField != null)
            {
                connectionDefaultColor = urlInputField.textComponent.color;
            }
            ToggleButtons(false);
            UpdateUI(defaultSettings);
        }

        /// <summary>
        /// Initialize the user interface and start video streaming if autoStartStreaming is true.
        /// </summary>
        protected virtual void Start()
        {
            if (autoStartStreaming)
            {
                UpdateVideoStreamSettings();
                SetUrl();
                StartVideo();
            }
        }

        /// <summary>
        /// Register to IP camera events.
        /// </summary>
        protected virtual void OnEnable()
        {
            ipCameraStream.ConnectionFailed += OnConnectionFailed;
            ipCameraStream.ConnectionSucceeded += OnConnectionSucceeded;
            ipCameraStream.ConnectionClosed += OnConnectionClosed;
        }

        /// <summary>
        /// Unregister from IP camera events.
        /// </summary>
        protected virtual void OnDisable()
        {
            ipCameraStream.ConnectionFailed -= OnConnectionFailed;
            ipCameraStream.ConnectionSucceeded -= OnConnectionSucceeded;
            ipCameraStream.ConnectionClosed -= OnConnectionClosed;
        }

        protected virtual void Update()
        {
            // TODO: maybe some code could be added here in future (e.g. timeout timer visualization)
        }
    }
}
