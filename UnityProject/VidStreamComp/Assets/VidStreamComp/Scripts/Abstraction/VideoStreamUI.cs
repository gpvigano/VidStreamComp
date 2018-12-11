using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// Basic UI for video streaming control buttons.
    /// </summary>
    public class VideoStreamUI : MonoBehaviour
    {
        [Tooltip("Button for starting the video streaming (optional)")]
        [SerializeField]
        protected Button startButton;

        [Tooltip("Button for stopping the video streaming (optional)")]
        [SerializeField]
        protected Button stopButton;

        [Tooltip("Button for pausing the video streaming (optional)")]
        [SerializeField]
        protected Button pauseButton;

        /// <summary>
        /// Called by derived class methods on Play/Stop/... events.
        /// </summary>
        /// <param name="started">Video streaming has been started.</param>
        /// <param name="paused">Video streaming has been paused.</param>
        protected virtual void ToggleButtons(bool started, bool paused = false)
        {
            if (startButton != null)
            {
                startButton.interactable = !started || paused;
            }
            if (stopButton != null)
            {
                stopButton.interactable = started;
            }
            if (pauseButton != null)
            {
                pauseButton.interactable = started && !paused;
            }
        }
    }
}
