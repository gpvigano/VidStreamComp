using UnityEngine;
using System;
using System.Xml.Serialization;

namespace VidStreamComp
{
    [Serializable]
    [XmlType(TypeName = "video-streaming-settings")]
    public class VideoStreamingSettings
    {
        [Tooltip("URL of the video stream")]
        public string videoUrl = null;
        [Tooltip("Optional login for the URL of the video stream")]
        public string login = null;
        [Tooltip("Optional password for the URL of the video stream")]
        public string password = null;
    }
}
