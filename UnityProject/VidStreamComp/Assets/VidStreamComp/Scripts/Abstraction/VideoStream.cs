using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VidStreamComp
{
    /// <summary>
    /// Abstract class for video streaming.
    /// </summary>
    public abstract class VideoStream : MonoBehaviour
    {
        /// <summary>
        /// Start video on startup.
        /// </summary>
        [Tooltip("Start video on startup")]
        [SerializeField]
        protected bool autoStartVideo = true;

        private int savedFrameWidth = 0;
        private int savedFrameHeight = 0;

        /// <summary>
        /// To be defined for playing state (read only).
        /// </summary>
        public abstract bool Playing { get; }

        /// <summary>
        /// To be defined for pausing state (read only).
        /// </summary>
        public abstract bool Paused { get; }

        /// <summary>
        /// To be defined for initialization state (read only).
        /// </summary>
        public abstract bool Initialized { get; }

        /// <summary>
        /// Texture of the frame buffer built by the video implementation.
        /// </summary>
        public abstract Texture FrameTexture { get; }

        /// <summary>
        /// Width of the frame buffer, defined by the video implementation.
        /// </summary>
        public abstract int FrameWidth { get; }

        /// <summary>
        /// Height of the frame buffer, defined by the video implementation.
        /// </summary>
        public abstract int FrameHeight { get; }

        /// <summary>
        /// Event triggered after initialization.
        /// </summary>
        public event Action InitializedEvent;

        /// <summary>
        /// Event triggered when Play() is called.
        /// </summary>
        public event Action PlayEvent;

        /// <summary>
        /// Event triggered when the video frame has changed.
        /// </summary>
        public event Action FrameUpdateEvent;

        /// <summary>
        /// Event triggered when the video frame size has changed.
        /// </summary>
        public event Action FrameResizeEvent;

        /// <summary>
        /// Event triggered when Pause() is called.
        /// </summary>
        public event Action PauseEvent;

        /// <summary>
        /// Event triggered when Stop() is called.
        /// </summary>
        public event Action StopEvent;

        /// <summary>
        /// Event triggered when Terminate() is called.
        /// </summary>
        public event Action TerminateEvent;

        /// <summary>
        /// The video frame has changed since last (Unity) frame.
        /// </summary>
        protected abstract bool FrameTextureChanged { get; }

        /// <summary>
        /// Play frames from the video source (initialize if not yet initialized).
        /// </summary>
        public void Play()
        {
            if (!Initialized)
            {
                Initialize();
            }
            if (Initialized)
            {
                PlayVideo();
                OnPlay();
            }
        }

        /// <summary>
        /// Pause the video if playing.
        /// </summary>
        public void Pause()
        {
            if (Initialized && Playing && !Paused)
            {
                PauseVideo();
                OnPause();
            }
        }

        /// <summary>
        /// Stop the video if playing or paused.
        /// </summary>
        public void Stop()
        {
            if (Initialized && (Playing || Paused))
            {
                StopVideo();
                OnStop();
            }
        }

        /// <summary>
        /// Initialize calling InitializeVideo(), InitializeMaterials(), OnInitialize().
        /// </summary>
        /// <remarks>Usually it is enough to override InitializeVideo() and TerminateVideo()</remarks>
        /// <returns>It returns true on success, false on failure.</returns>
        public virtual bool Initialize()
        {
            if (!InitializeVideo())
            {
                return false;
            }
            OnInitialize();
            return true;
        }

        /// <summary>
        /// Terminate video calling TerminateVideo() and reset materials.
        /// </summary>
        public virtual void Terminate()
        {
            if (Initialized)
            {
                Stop();
                TerminateVideo();
                OnTerminate();
            }
        }

        /// <summary>
        /// Abstract method to be overridden for video initialization.
        /// </summary>
        /// <returns>It must return true on success, false on failure.</returns>
        protected abstract bool InitializeVideo();

        /// <summary>
        /// Abstract method to be overridden for terminating the video (free resources).
        /// </summary>
        protected abstract void TerminateVideo();

        /// <summary>
        /// Abstract method to be overridden for playing the video from the implemented source.
        /// </summary>
        protected abstract void PlayVideo();

        /// <summary>
        /// Abstract method to be overridden for pausing the video.
        /// </summary>
        protected abstract void PauseVideo();

        /// <summary>
        /// Abstract method to be overridden for stopping the video.
        /// </summary>
        protected abstract void StopVideo();

        /// <summary>
        /// Trigger the InitializedEvent event.
        /// </summary>
        protected virtual void OnInitialize()
        {
            if (InitializedEvent != null)
            {
                InitializedEvent();
            }
        }

        /// <summary>
        /// Trigger the PlayEvent event.
        /// </summary>
        protected virtual void OnPlay()
        {
            if (PlayEvent != null)
            {
                PlayEvent();
            }
        }

        /// <summary>
        /// Trigger the FrameUpdateEvent event.
        /// </summary>
        protected virtual void OnFrameUpdate()
        {
            if (FrameUpdateEvent != null)
            {
                FrameUpdateEvent();
            }
        }

        /// <summary>
        /// Trigger the FrameResizeEvent event.
        /// </summary>
        protected virtual void OnFrameResize()
        {
            if (FrameResizeEvent != null)
            {
                FrameResizeEvent();
            }
        }

        /// <summary>
        /// Trigger the PauseEvent event.
        /// </summary>
        protected virtual void OnPause()
        {
            if (PauseEvent != null)
            {
                PauseEvent();
            }
        }

        /// <summary>
        /// Trigger the StopEvent event.
        /// </summary>
        protected virtual void OnStop()
        {
            if (StopEvent != null)
            {
                StopEvent();
            }
        }

        /// <summary>
        /// Trigger the TerminateEvent event.
        /// </summary>
        protected virtual void OnTerminate()
        {
            if (TerminateEvent != null)
            {
                TerminateEvent();
            }
        }

        /// <summary>
        /// Play the video on startup if autoStartVideo is true.
        /// </summary>
        protected virtual void Start()
        {
            if (autoStartVideo)
            {
                Play();
            }
        }

        /// <summary>
        /// Call Teminate() on application quit.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            Terminate();
        }

        /// <summary>
        /// On enalble play the video if autoStartVideo is true.
        /// </summary>
        protected virtual void OnEnable()
        {
            // OnEnable() is first called on scene load,
            // then the video is played by Start().
            // Here we play it only if it was disabled
            // after Start()
            if (Initialized && !Playing && autoStartVideo)
            {
                PlayVideo();
            }
        }

        /// <summary>
        /// On enalble stop the video.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (Playing || Paused)
            {
                StopVideo();
            }
        }

        /// <summary>
        /// Call Teminate() on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            TerminateVideo();
        }

        protected virtual void Update()
        {
            if (FrameTextureChanged)
            {
                OnFrameUpdate();
            }
            if (savedFrameWidth != FrameWidth || savedFrameHeight != FrameHeight)
            {
                OnFrameResize();
            }
        }
    }
}
