using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    [RequireComponent(typeof(WebcamStream))]
    public class WebcamStreamUI : VideoStreamUI
    {
        [Tooltip("Dropdown list for webcam devices")]
        [SerializeField]
        private Dropdown deviceDropdown;
        [Tooltip("Text for displaying the active webcam")]
        [SerializeField]
        private Text webcamDeviceText;
        [Tooltip("Color for the URL input field when the connection fails.")]
        [SerializeField]
        private Color initializationFailedColor = Color.red;
        [Tooltip("Color for the URL input field when the connection succeeds.")]
        [SerializeField]
        private Color initializationSucceededColor = Color.blue;
        [Tooltip("Add an invalid entry in device list (to deactivate the webcam).")]
        [SerializeField]
        private bool addDisabledDevice = false;
        [Tooltip("Show the number of frames acquired in the Webcam Device Text.")]
        [SerializeField]
        private bool showFramesCount = false;

        private WebcamStream webcamStream;

        private List<string> webcamDeviceNames = new List<string>();
        private Color textDefaultColor = Color.black;
        private Color textCurrentColor = Color.black;

        /// <summary>
        /// Change the device given a choice index (UI Dropdown).
        /// </summary>
        /// <param name="index">Index of the choice in the device dropdown list</param>
        public void ChangeDevice(int index)
        {
            string deviceName = deviceDropdown.options[index].text;
            foreach (WebCamDevice device in WebCamTexture.devices)
            {
                if (device.name == deviceName)
                {
                    webcamStream.DeviceName = deviceName;
                    textCurrentColor = initializationSucceededColor;
                    UpdateDeviceListText();
                    UpdateDeviceTextColor();
                    webcamStream.Play();
                    return;
                }
            }
            webcamStream.Terminate();
            textCurrentColor = initializationFailedColor;
            if (webcamDeviceText != null)
            {
                webcamDeviceText.text = deviceName;
            }
            UpdateDeviceTextColor();
        }

        /// <summary>
        /// Wrapper method forplay/pause from UI toggles.
        /// </summary>
        /// <param name="playing">If true play, else pause.</param>
        public void Play(bool playing)
        {
            if (playing)
            {
                webcamStream.Play();
            }
            else
            {
                webcamStream.Pause();
            }
            ToggleButtons(playing);
        }

        /// <summary>
        /// Update the UI text with the devices list, the active one is highlighted.
        /// </summary>
        public void UpdateDeviceList()
        {
            webcamDeviceNames.Clear();
            foreach (WebCamDevice device in WebCamTexture.devices)
            {
                webcamDeviceNames.Add(device.name);
            }
            if (addDisabledDevice)
            {
                webcamDeviceNames.Add("(disabled)");
            }
            UpdateDeviceListText();
            if (deviceDropdown != null)
            {
                deviceDropdown.ClearOptions();
                deviceDropdown.AddOptions(webcamDeviceNames);
                deviceDropdown.captionText.text = webcamStream.DeviceName;
            }
        }

        private void UpdateDeviceListText()
        {
            if (webcamDeviceText != null)
            {
                webcamDeviceText.text = webcamStream.DeviceName;
            }
        }

        private void UpdateDeviceTextColor()
        {
            if (webcamDeviceText != null)
            {
                webcamDeviceText.color = textCurrentColor;
            }
            if (deviceDropdown != null)
            {
                deviceDropdown.captionText.color = textCurrentColor;
            }
        }

        private void Awake()
        {
            webcamStream = GetComponent<WebcamStream>();
        }

        private void Start()
        {
            UpdateDeviceList();
            UpdateDeviceTextColor();
        }

        private void OnEnable()
        {
            webcamStream.InitializedEvent += OnWebcamInitialization;
            webcamStream.TerminateEvent += OnWebcamTermination;
            webcamStream.PlayEvent += OnPlayEvent;
            webcamStream.PauseEvent += OnPauseEvent;
            webcamStream.StopEvent += OnStopEvent;
        }

        private void OnDisable()
        {
            webcamStream.InitializedEvent -= OnWebcamInitialization;
            webcamStream.TerminateEvent -= OnWebcamTermination;
            webcamStream.PlayEvent -= OnPlayEvent;
            webcamStream.PauseEvent -= OnPauseEvent;
            webcamStream.StopEvent -= OnStopEvent;
        }

        private void OnPlayEvent()
        {
            ToggleButtons(true);
        }

        private void OnPauseEvent()
        {
            ToggleButtons(true, true);
        }

        private void OnStopEvent()
        {
            ToggleButtons(false);
        }

        private void OnWebcamInitialization()
        {
            textCurrentColor = initializationSucceededColor;
            UpdateDeviceList();
            UpdateDeviceTextColor();
        }

        private void OnWebcamTermination()
        {
            textCurrentColor = textDefaultColor;
            UpdateDeviceTextColor();
            ToggleButtons(false);
        }

        private void OnWebCamInitialization()
        {
            UpdateDeviceList();
        }

        private void Update()
        {
            if (showFramesCount && webcamDeviceText != null && webcamStream.Playing)
            {
                webcamDeviceText.text = webcamStream.DeviceName + "\nFrames: " + GetComponent<WebcamComponent>().FrameCounter;
            }
        }
    }
}
