# VidStreamComp
### Components for video streaming in Unity
**Requires Unity 2017.1 or higher.**

A self-contained package for controlling video streaming in Unity.
This is a collection of Unity components (scripts) for implementing video streaming in Unity, also over IP.

**Sample scenes included.**

 ![image](https://raw.githubusercontent.com/gpvigano/VidStreamComp/master/images/VidStreamComp_webcam_Android.png)

*The webcam example scene running on an Android smartphone.*

## Features
What's included:
* Main scripts:
  * `WebcamStream`
    * control the webcam
    * see the example scene `WebcamStream`
  * `IPCameraStream`
    * control a remote (or local) IP camera using MJPEG protocol
    * see the example scene `IPCameraStream`
  * `IPWebcamStreamer`
    * start as HTTP server and listen for client connections
	* stream video frames using MJPEG protocol
    * see the example scene `IPWebcamStreamer`
* Scripts for UI control:
  * `WebcamStreamUI`
    * UI control script for `WebcamStream`
	* control buttons
	* device selection
    * see the example scene `WebcamStream`
  * `IPCameraStream`
	* connection configuration
	* control buttons
    * UI control script for `IPCameraStream`
    * see the example scene `IPCameraStream`
  * `IPWebcamStreamer`
    * UI control script for `IPWebcamStreamer`
	* connection configuration
	* control buttons
	* button for opening a local browser page connected to a video stream (e.g. for testing)
    * see the example scene `IPWebcamStreamer`
* Scripts for UI rendering:
  * `VideoStreamToImage`: rendering of a video streaming to a material of an Image.
  * `VideoStreamToRenderer`: rendering of a video streaming to a material of a Renderer (e.g. `MeshRenderer`, see the small cube in the example scenes).
* Utility scripts:
  * `WebcamComponent`: wrapper for `UnityEngine.WebCamTexture`
  * `FrameImageResizer`: resizing an Image Transform to keep video frames aspect.
  * `VideoStreamingSettings`: serializable data for a video streaming connection.

 ![image](https://raw.githubusercontent.com/gpvigano/VidStreamComp/master/images/VidStreamComp_IP-browser.png)

*Browser window connected to the `IPWebcamStreamer` example application.*

## Tested platforms:
* Windows Standalone (x86/x64)
* Android
* Universal Windows Platform (all the examples compile and run, but `IPWebcamStreamer` does not work yet, mainly due to the limited .NET support of this platform)


## Documentation

Some example scenes are provided to demonstrate different usages of the components.

To see how these component work, see example scenes in `Assets/VidStreamComp/Scenes`.

### Acknowledgements:

The first implementation was born within [GPVUDK](https://github.com/gpvigano/GPVUDK).
Part of `IPCameraStream` is inspired by the code posted [here](https://answers.unity.com/questions/1151512/show-video-from-ip-camera-source.html).
Part of `IPWebcamStreamer` is inspired by the code posted [here](https://answers.unity.com/questions/1245582/create-a-simple-http-server-on-the-streaming-asset.html) and  [here](http://www.ridgesolutions.ie/index.php/2014/11/24/streaming-mjpeg-video-with-web2py-and-python/).
Thanks in advance to all the people who contributed and will contribute in any way to this project.


### Contributing

Contributions from you are welcome!

If you find bugs or you have any new idea for improvements and new features you can raise an issue on GitHub (please follow the suggested template, filling the proper sections). To open issues or make pull requests please follow the instructions in [CONTRIBUTING.md](https://github.com/gpvigano/VidStreamComp/blob/master/CONTRIBUTING.md).

### License

Code released under the [MIT License](https://github.com/gpvigano/VidStreamComp/blob/master/LICENSE.txt).


---
To try this project with Unity go to the [repository page on GitHub](https://github.com/gpvigano/VidStreamComp) press the button **Clone or download** and choose [**Download ZIP**](https://github.com/gpvigano/VidStreamComp/archive/master.zip). Save and unzip the archive to your hard disk and then you can open it with Unity, selecting the sub-folder UnityProject/VidStreamComp.


