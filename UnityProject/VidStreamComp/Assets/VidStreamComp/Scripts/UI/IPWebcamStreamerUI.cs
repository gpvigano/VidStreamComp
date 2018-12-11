using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// User interface manager for IP webcam streamer configuration.
    /// </summary>
    [RequireComponent(typeof(IPWebcamStreamer))]
    public class IPWebcamStreamerUI : MonoBehaviour
    {
        [Tooltip("Input field for the IP webcam streaming port (0 = first free)")]
        [SerializeField]
        private InputField portInputField;
        [Tooltip("Readonly input field for the IP webcam streaming URL (MJPEG video stream, not an HTTP page)")]
        [SerializeField]
        private InputField urlTextField;
        [Tooltip("Dropdown list for local host address format")]
        [SerializeField]
        private Dropdown addressDropdown;
        [Tooltip("Button for opening a browser window to connect to this camera stremer.")]
        [SerializeField]
        private Button openBrowserButton;

        private IPWebcamStreamer webcamStreamer;
        private string localAddress = "localhost";
        private string urlText;

        /// <summary>
        /// Open a browser window connected to the IP webcam streamer
        /// </summary>
        public void OpenBrowserWindow()
        {
            if(!string.IsNullOrEmpty(urlText))
            {
                Application.OpenURL(urlText);
            }
        }

        /// <summary>
        /// Change the server port converting the given string to a number (UI InputField).
        /// </summary>
        /// <param name="portText">Port number as text from an input field</param>
        public void ChangePort(string portText)
        {
            int serverPort = webcamStreamer.ServerPort;
            int.TryParse(portText, out serverPort);
            webcamStreamer.ServerPort = serverPort;

            UpdateUrlText();
        }

        /// <summary>
        /// Change the server address given a choice index (UI Dropdown).
        /// </summary>
        /// <param name="index">Index of the choice in the addres dropdown list</param>
        public void ChangeAddress(int index)
        {
            localAddress = addressDropdown.options[index].text;
            UpdateUrlText();
        }

        private void UpdateUrlText()
        {
            if (webcamStreamer.ServerPort > 0)
            {
                urlText = "http://" + localAddress + ":" + webcamStreamer.ServerPort + "/";
                urlTextField.text = urlText;
                urlTextField.interactable = true;
                openBrowserButton.interactable = true;
            }
        }

        private void OnEnable()
        {
            webcamStreamer.StreamingStarted += WebcamStreamer_StreamingStarted;
            webcamStreamer.StreamingStopped += WebcamStreamer_StreamingStopped;
        }

        private void WebcamStreamer_StreamingStopped()
        {
            portInputField.readOnly = false;
        }

        private void WebcamStreamer_StreamingStarted()
        {
            UpdateUrlText();
            portInputField.text = webcamStreamer.ServerPort.ToString();
            portInputField.readOnly = true;
        }

        private List<string> GetLocalAddresses()
        {
            List<string> addressList = new List<string>();
#if UNITY_WSA_10_0 && WINDOWS_UWP
                // TODO: implement UWP version
#else
            string hostName = Dns.GetHostName();
            addressList.Add("localhost");
            addressList.Add(hostName);
            IPHostEntry host = Dns.GetHostEntry(hostName);
            // search for IPv4 addresses
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    addressList.Add(ip.ToString());
                }
            }
#endif
            return addressList;
        }
        private void Awake()
        {
            webcamStreamer = GetComponent<IPWebcamStreamer>();
            portInputField.interactable = true;
            if (webcamStreamer.ServerPort > 0)
            {
                portInputField.text = webcamStreamer.ServerPort.ToString();
            }
            urlTextField.interactable = false;
            urlTextField.readOnly = true;
            openBrowserButton.interactable = false;
            addressDropdown.ClearOptions();
            addressDropdown.AddOptions(GetLocalAddresses());
            UpdateUrlText();
        }
    }
}
