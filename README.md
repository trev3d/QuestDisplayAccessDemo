# Quest Display Access Demo

Developers want camera access on the Meta Quest. Meta hasn't let us have it yet. The next best thing is display access. Thanks to Android's MediaProjector API, you can copy the display image to a texture in your Unity project in 'near realtime' (several frames of latency) as demonstrated in this horribly architected Unity project. No PC, embedded browser, or dev mode required

![apriltag demo](https://github.com/user-attachments/assets/3132a917-7472-4dc5-aa51-0416a6551e62)

## ⚠️ Issues (please read)!

### To fix 

⚠️ MediaProjection stop callback doesn't seem to work correctly

⚠️ Head pose isn't correct for AprilTag tracking. Tag positions 'flicker' as you move or rotate your head. I need to somehow reliably get the head pose at the time the latest screen frame was captured but I haven't figured out how to do this.

### Gotchas

⚠️ To set this up in an existing project, you'll need the app to launch with the `UnityPlayerActivityWithMediaProjector` activity. To set this up you need to modify your `AndroidManifest` file. For more info, see [this page](https://docs.unity3d.com/Manual/android-custom-activity.html).

⚠️ Expect ~10% CPU usage when display capture is enabled and another ~30% of CPU usage on tag tracking. This demo is horribly optimized!

⚠️ You may need to be on Quest system software v68 or higher 

⚠️ This only works on-headset. This will not work through QuestLink

⚠️ You cannot video record the display 'normally' while this app's MediaProjector session is running. You can instead use [scrcpy](https://github.com/Genymobile/scrcpy) to record any prototypes or demos you make with this.

⚠️ This still isn't proper camera access. Any virtual elements will obscure physical objects in the image. If you need to track something, you must not render anything on top of it!

### AprilTag tracking

- This project contains a modified version of [Keijiro Takahashi's AprilTag package](https://github.com/keijiro/jp.keijiro.apriltag) to not vertically flip the incoming texture (as it is already 'flipped' ).
- Only works with `tagStandard41h12` tag set.
- Set up in project to work with 12cm tags (measuring the inner white square) which is about what you'd get if you printed a tag on 8.5" by 11" letter paper 

### Other info

- The captured view is ~82 degrees in horizontal and vertical FOV on Quest 3
- The capture texture is 1024x1024, at least on Quest 3
- The left eye buffer is where captured frames come from
- Quest system camera settings do not affect the capture resolution, framerate, or eye side

## Reference

- [https://developer.oculus.com/documentation/native/native-media-projection/](https://developer.oculus.com/documentation/native/native-media-projection/)
- [https://developer.android.com/media/grow/media-projection](https://developer.android.com/media/grow/media-projection)
- [https://github.com/android/media-samples/tree/main/ScreenCapture](https://github.com/android/media-samples/tree/main/ScreenCapture)
