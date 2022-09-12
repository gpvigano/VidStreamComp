using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text;
#if UNITY_WSA_10_0 && WINDOWS_UWP
using System.Net.Http;
#else
using System.Net;
#endif
using System.Net.Sockets;
using System.IO;

namespace VidStreamComp
{
    /// <summary>
    /// Acquire images from a video stream coming from a remote IP camera
    /// </summary>
    public class IPCameraStream : VideoStream
    {
        [SerializeField]
        private string url = "http://10.2.13.100:8080/video";
        [Tooltip("Optional login required to access the IP camera")]
        [SerializeField]
        private string login = null;
        [Tooltip("Optional password required to access the IP camera")]
        [SerializeField]
        private string password = null;
        [Tooltip("Authentication scheme required to access the IP camera")]
        [SerializeField]
        private string authentication = "Basic";
        [Tooltip("Restart on connection lost")]
        [SerializeField]
        private bool restartOnError = true;
        [Tooltip("Timeout for video stream reading operations")]
        [SerializeField]
        private float readTimeout = 0.2f;
        [Tooltip("Time before giving up after read timeouts")]
        [SerializeField]
        private float giveUpTimeout = 10f;
        [Tooltip("Time before retrying if the connection is lost")]
        [SerializeField]
        private float reconnectTimeout = 1f;

        private Texture2D frameTexture;
        // Internal buffer
        private Byte[] jpegData;

        private string thisClassName;
        private bool firstFrame = false;
        private bool restartStream = false;
        private bool isPaused = false;
        private bool isUpdatedSinceLastFrame = false;

        /// <summary>
        /// Flag set to true when the video streming is running.
        /// </summary>
        public bool IsStreaming { get; private set; }

        public override bool Initialized
        {
            get
            {
                return frameTexture != null;// && frameTexture.width > 2;
            }
        }

        public override bool Playing
        {
            get
            {
                return Initialized && IsStreaming && !isPaused;
            }
        }

        public override bool Paused
        {
            get
            {
                return Initialized && isPaused;
            }
        }

        public override int FrameWidth
        {
            get
            {
                return frameTexture.width;
            }
        }

        public override int FrameHeight
        {
            get
            {
                return frameTexture.height;
            }
        }

        public override Texture FrameTexture
        {
            get
            {
                return frameTexture;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }

            set
            {
                SetUrl(value, login, password);
            }
        }

        public string Login
        {
            get
            {
                return login;
            }

            set
            {
                SetUrl(url, value, password);
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                SetUrl(url, login, value);
            }
        }

        protected override bool FrameTextureChanged
        {
            get
            {
                return isUpdatedSinceLastFrame;
            }
        }

        /// <summary>
        /// Event triggered when on connection error.
        /// </summary>
        public event Action ConnectionFailed;
        /// <summary>
        /// Event triggered when the connection succeeds.
        /// </summary>
        public event Action ConnectionSucceeded;
        /// <summary>
        /// Event triggered when the connection is closed.
        /// </summary>
        public event Action ConnectionClosed;

        /// <summary>
        /// Set URL and optionally login and password for the IP camera.
        /// </summary>
        /// <param name="url">URL of the IP camera</param>
        /// <param name="login">Optional login required to access the IP camera</param>
        /// <param name="password">Optional password required to access the IP camera</param>
        public void SetUrl(string url, string login = null, string password = null)
        {
            bool wasStreaming = IsStreaming;
            StopStream();
            this.url = url;
            this.login = login;
            this.password = password;
            if (wasStreaming)
            {
                StartStream();
            }
        }

        /// <summary>
        /// Stop video streaming, if running.
        /// </summary>
        protected virtual void StopStream()
        {
            if (IsStreaming)
            {
                Debug.Log(thisClassName + ": stream stopped");
            }
            IsStreaming = false;
            StopCoroutine(GetVideoStream());
        }

        /// <summary>
        /// Start video streaming, restart if already running.
        /// </summary>
        protected virtual void StartStream()
        {
            StartCoroutine(GetVideoStream());
        }

        /// <summary>
        /// Fire ConnectionFailed event.
        /// </summary>
        protected virtual void OnConnectionFailed()
        {
            if (ConnectionFailed != null)
            {
                ConnectionFailed();
            }
        }

        /// <summary>
        /// Fire ConnectionSucceeded event.
        /// </summary>
        protected virtual void OnConnectionSucceeded()
        {
            if (ConnectionSucceeded != null)
            {
                ConnectionSucceeded();
            }
        }

        /// <summary>
        /// Fire ConnectionFailed event, wait and restart video streaming.
        /// </summary>
        protected virtual IEnumerator OnStreamLost()
        {
            OnConnectionFailed();
            if (restartOnError)
            {
                yield return new WaitForSecondsRealtime(reconnectTimeout);
                restartStream = true;
            }
        }

        /// <summary>
        /// Get the video stream in a coroutine.
        /// </summary>
        /// <returns></returns>
#if UNITY_WSA_10_0 && WINDOWS_UWP
        private IEnumerator GetVideoStream()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(readTimeout);
                Stream stream = httpClient.GetStreamAsync(url).Result;

                using (StreamReader reader = new StreamReader(stream))
                {
                    yield return GetFrames(stream);
                }
            }
            Debug.Log("Stream closed.");
        }
#else
        private IEnumerator GetVideoStream()
        {
            Stream responseStream;
            WebResponse webResponse;
            restartStream = false;
            // create HTTP request
            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ReadWriteTimeout = 3000;
            }
            catch (FormatException e)
            {
                OnConnectionFailed();
                Debug.LogErrorFormat("Wrong format for IP camera URL: {0}", e);
                yield break;
            }

            try
            {
                bool loginDefined = !string.IsNullOrEmpty(login);
                if (loginDefined)
                {
                    NetworkCredential credential = new NetworkCredential(login, password);
                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new Uri(url), authentication, credential);
                    httpWebRequest.Credentials = credentialCache;
                }
                // get response
                webResponse = httpWebRequest.GetResponse();
            }
            catch (WebException e)
            {
                OnConnectionFailed();
                Debug.LogErrorFormat("Failed to connect to IP camera: {0}", e);
                yield break;
            }

            // get response stream
            using (responseStream = webResponse.GetResponseStream())
            {
                yield return GetFrames(responseStream);
            }
            webResponse.Close();
            httpWebRequest.Abort();
            if(ConnectionClosed!=null)
            {
                ConnectionClosed();
            }
            Debug.Log("Stream closed.");
            //StopStream();
        }
#endif

        private IEnumerator GetFrames(Stream stream)
        {
            // TODO: this method must be reviewed

#if UNITY_WSA_10_0 && WINDOWS_UWP
#else
            stream.ReadTimeout = (int)(readTimeout * 1000f);
#endif
            IsStreaming = true;
            firstFrame = true;
            float timeoutStartTime = 0;

            while (IsStreaming)
            {
                if (isPaused)
                {
                    // TODO: check if ignored frames cause a delay later when resumed
                    yield return null;
                    continue;
                }
                bool frameGot = false;
                bool streamLost = false;
                try
                {
                    float elapsedTime = 0;
                    int bytesToRead = -1;
                    try
                    {
                        bytesToRead = ReadDataLength(stream);
                    }
#if UNITY_WSA_10_0 && WINDOWS_UWP
                    catch (IOException)
#else
                    catch (WebException)
#endif
                    {
                        // Error handling is made in the "if (bytesToRead == -1)" block
                    }
                    catch (ObjectDisposedException)
                    {
                        streamLost = true;
                    }
                    if (streamLost)
                    {
                        Debug.LogError("Stream lost while reading header.");
                        yield return OnStreamLost();
                        yield break;
                    }
                    if (bytesToRead == -1)
                    {
                        if (timeoutStartTime == 0)
                        {
                            timeoutStartTime = Time.realtimeSinceStartup;
                        }
                        Debug.Log(thisClassName + ": waiting for frames.");
                        //                print("End of stream");
                        yield return null;
                        elapsedTime += Time.realtimeSinceStartup - timeoutStartTime;
                        if (elapsedTime < giveUpTimeout)
                        {
                            yield return new WaitForEndOfFrame();
                            continue;
                        }
                        else
                        {
                            timeoutStartTime = 0;
                            Debug.LogErrorFormat("Connection from IP camera lost after {0:F1} seconds.", elapsedTime);
                            yield return OnStreamLost();
                            yield break;
                        }
                    }
                    if (jpegData.Length < bytesToRead)
                    {
                        // if the internal buffer is not enough double it
                        jpegData = new byte[bytesToRead * 2];
                    }

                    int leftToRead = bytesToRead;
                    bool stillReading = true;
                    // no trailing newline, see:
                    // https://stackoverflow.com/questions/13821263/should-newline-be-included-in-http-response-content-length
                    //int trailToRead = 2;
                    //byte[] trailBuf = new byte[2];

                    while (stillReading && IsStreaming)
                    {
                        bool timeout = false;
                        //print(leftToRead);
                        try
                        {
                            if (leftToRead > 0)
                            {
                                leftToRead -= stream.Read(
                                    jpegData, bytesToRead - leftToRead, leftToRead);
                            }
                            //else if (trailToRead > 0)
                            //{
                            //    trailToRead -= stream.Read(
                            //        trailBuf, 2 - trailToRead, trailToRead);
                            //    //stream.ReadByte(); // CR after bytes
                            //    //stream.ReadByte(); // LF after bytes
                            //}
                            else
                            {
                                stillReading = false;
                            }
                        }
#if UNITY_WSA_10_0 && WINDOWS_UWP
                        catch (IOException)
#else
                        catch (WebException)
#endif
                        {
                            // Probably the operation has timed out. Retry in the next iteration.
                            timeout = true;
                        }
                        catch (ObjectDisposedException)
                        {
                            streamLost = true;
                        }

                        if (timeout)
                        {
                            Debug.Log("Timeout.");
                            yield return null;
                            continue;
                        }
                        if (streamLost)
                        {
                            Debug.LogError("Stream lost.");
                            yield return OnStreamLost();
                            yield break;
                        }
                        yield return null;
                    }

                    if (IsStreaming)
                    {
                        //Debug.Log(thisClassName + ": Frame read");
                        //#if UNITY_WSA_10_0 && WINDOWS_UWP
                        frameTexture.LoadImage(jpegData);
                        isUpdatedSinceLastFrame = true;
                        //#else
                        //                    MemoryStream ms = new MemoryStream(jpegData, 0, bytesToRead, false, true);
                        //                    texture.LoadImage(ms.GetBuffer());
                        //#endif
                        frameGot = true;
                        OnConnectionSucceeded();
                        firstFrame = false;
                    }
                }
                finally
                {
                    if (IsStreaming && !frameGot)
                    {
                        Debug.LogWarning("Frame missed.");
                    }
                }
            }
        }

        /// <summary>
        /// Read the message header.
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>Returns the whole header text</returns>
        private string ReadHeader(Stream stream)
        {
            int b = 0;
            bool atEndOfHeader = false;
            bool headerComplete = false;
            StringBuilder headerBuilder = new StringBuilder();
            while (IsStreaming && !headerComplete)
            {
                b = stream.ReadByte();
                if (b == -1)
                {
                    return null;
                }
                headerBuilder.Append((char)b);
                if (b != 10)
                {
                    if (b == 13)
                    {
                        if (atEndOfHeader)
                        {
                            headerBuilder.Append((char)stream.ReadByte());
                            headerComplete = true;
                        }
                        atEndOfHeader = true;
                    }
                    else
                    {
                        atEndOfHeader = false;
                    }
                }
            }
            return headerBuilder.ToString();
        }

        /// <summary>
        /// Extract a value from the header text.
        /// </summary>
        /// <param name="header">header text</param>
        /// <param name="tag">tag to be read</param>
        /// <param name="startIdx">index in the header text</param>
        /// <returns>Returns the requested value or null if not found.</returns>
        private string ExtractHeaderValue(string header, string tag, int startIdx)
        {
            int tagLength = tag.Length;
#if UNITY_WSA_10_0 && WINDOWS_UWP
            int tagIdx = header.IndexOf(tag, startIdx, StringComparison.CurrentCultureIgnoreCase);
#else
            int tagIdx = header.IndexOf(tag, startIdx, StringComparison.InvariantCultureIgnoreCase);
#endif
            if (tagIdx == -1)
            {
                return null;
            }
            int nlIdx = header.IndexOf((char)13, tagIdx + tagLength);
            if (nlIdx == -1)
            {
                return null;
            }
            int valIdx = tagIdx + tagLength;
            string valString = header.Substring(valIdx, nlIdx - valIdx).Trim();
            return valString;
        }

        /// <summary>
        /// Read the header to detect the number of bytes to read.
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <returns>The number of bytes to read or -1 on error.</returns>
        private int ReadDataLength(Stream stream)
        {
            int result = -1;
            const string typeTag = "Content-Type:";
            const string lengthTag = "Content-Length:";
            int lengthTagLength = lengthTag.Length;

            string header = ReadHeader(stream);
            if (header == null)
            {
                return -1;
            }
            if (firstFrame)
            {
                Debug.LogFormat("{0} - frame header read: {1}", thisClassName, header);
            }
            result = Convert.ToInt32(ExtractHeaderValue(header, lengthTag, 0));
            string contentType = ExtractHeaderValue(header, typeTag, 0);
            if (contentType != "image/jpeg")
            {
                IsStreaming = false;
                Debug.LogErrorFormat("{0} - Content type non supported: {1}", thisClassName, contentType);
            }
            return result;
        }

        private void Awake()
        {
            IsStreaming = false;
            thisClassName = GetType().Name;
        }

        protected override void Start()
        {
            Initialize();
            if (autoStartVideo && !string.IsNullOrEmpty(url))
            {
                PlayVideo();
            }
        }

        protected override void Update()
        {
            if (restartStream)
            {
                StartStream();
            }
            base.Update();
            isUpdatedSinceLastFrame = false;
        }

        protected override bool InitializeVideo()
        {
            frameTexture = new Texture2D(2, 2);
            jpegData = new byte[106496];//1MB
            return true;
        }

        protected override void TerminateVideo()
        {
            StopStream();
            // TODO: Is this enough to free memory?
            frameTexture = null;
            jpegData = null;
        }

        protected override void PlayVideo()
        {
            isPaused = false;
            StartStream();
        }

        protected override void PauseVideo()
        {
            isPaused = true;
        }

        protected override void StopVideo()
        {
            StopStream();
        }
    }
}
