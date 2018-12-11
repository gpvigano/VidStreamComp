using UnityEngine;
using UnityEngine.UI;

namespace VidStreamComp
{
    /// <summary>
    /// Adjust the size of the frame image
    /// so that it matches the parent transform or the canvas size.
    /// </summary>
    /// <remarks>This needs to be controlled by another script.</remarks>
    public class FrameImageResizer : MonoBehaviour
    {
        private Image frameImage;

        private bool isResizing = false;
        private int frameWidth = 0;
        private int frameHeight = 0;

        private int savedFrameWidth = 0;
        private int savedFrameHeight = 0;
        private float savedCanvasWidth = 0;
        private float savedCanvasHeight = 0;

        /// <summary>
        /// Enable/disable resizing according to the current size.
        /// </summary>
        /// <param name="on">true=resizing, false=not resizing</param>
        public void SwitchResizing(bool on)
        {
            isResizing = on;
        }

        /// <summary>
        /// Set the Image to be resized.
        /// </summary>
        /// <param name="image">Image to be resized.</param>
        public void SetFrameImage(Image image)
        {
            frameImage = image;
        }

        /// <summary>
        /// Update the size of the source frame.
        /// </summary>
        /// <param name="width">width of the source frame</param>
        /// <param name="height">height of the source frame</param>
        public void UpdateFrameSize(int width, int height)
        {
            frameWidth = width;
            frameHeight = height;
        }

        private bool DetectImageSizeChanges()
        {
            if (isResizing)
            {
                if (savedFrameWidth != frameWidth || savedFrameHeight != frameHeight)
                {
                    savedFrameWidth = frameWidth;
                    savedFrameHeight = frameHeight;
                    return true;
                }
                if (frameImage != null && frameImage.canvas!= null)
                {
                    Rect canvasRect = frameImage.canvas.pixelRect;
                    if (savedCanvasWidth != canvasRect.width || savedCanvasHeight != canvasRect.height)
                    {
                        savedCanvasWidth = canvasRect.width;
                        savedCanvasHeight = canvasRect.height;
                        return true;
                    }
                }
            }
            return false;
        }

        private void AdjustImageSize()
        {
            if (isResizing && frameImage != null && frameImage.GetComponent<Canvas>() == null)
            {
                float widthToFit;
                float heightToFit;
                Transform parent = frameImage.rectTransform.parent;
                bool hasParentRectTransform = parent != null
                    && parent.GetComponent<RectTransform>() != null
                    && parent.GetComponent<Canvas>() == null;
                if (!hasParentRectTransform)
                {
                    widthToFit = frameImage.canvas.pixelRect.width;
                    heightToFit = frameImage.canvas.pixelRect.height;
                }
                else
                {
                    // the parent is a canvas
                    RectTransform rectTransform = frameImage.rectTransform.parent.GetComponent<RectTransform>();
                    widthToFit = rectTransform.rect.width;
                    heightToFit = rectTransform.rect.height;
                }
                float aspect = frameWidth / (float)frameHeight;
                float aspectToFit = widthToFit / heightToFit;
                //Debug.LogFormat("Frame size: {0},{1}  Image size: {2},{3}", FrameWidth, FrameHeight, widthToFit, heightToFit);
                if (frameImage != null && aspect > 0)
                {
                    float newWidth;
                    float newHeight;
                    // touch window from inside
                    if (aspect < aspectToFit)
                    {
                        newHeight = heightToFit;
                        newWidth = heightToFit * aspect;
                    }
                    else
                    {
                        newWidth = widthToFit;
                        newHeight = widthToFit / aspect;
                    }
                    // TODO: implement other modes?
                    //    // fill
                    //    newWidth = widthToFit;
                    //    newHeight = heightToFit;
                    //
                    //    // match witdh (masking required)
                    //    newWidth = widthToFit;
                    //    newHeight = widthToFit / aspect;
                    //
                    //    // match height (masking required)
                    //    newWidth = heightToFit * aspect;
                    //    newHeight = heightToFit;
                    //
                    //    // touch window from outside (masking required)
                    //    if (aspect > aspectToFit)
                    //    {
                    //        newHeight = heightToFit;
                    //        newWidth = heightToFit * aspect;
                    //    }
                    //    else
                    //    {
                    //        newWidth = widthToFit;
                    //        newHeight = widthToFit / aspect;
                    //    }
                    frameImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
                    frameImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
                }

            }
        }

        private void Update()
        {
            if (DetectImageSizeChanges())
            {
                AdjustImageSize();
            }
        }
    }
}
