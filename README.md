# Quest Display Access Demo

Developers want camera access on the Meta Quest. Meta hasn't let us have it yet. The next best thing is display access. Thanks to Android's MediaProjector API, you can copy the display image to a texture in your Unity project in 'near realtime' (several frames of latency) as demonstrated here. No PC, embedded browser, or dev mode required

![scrcpy_VmJvDrjcQL](https://github.com/user-attachments/assets/522bc5ea-8b91-4ee9-91cd-0385fffc93a3)

## Special thanks

[@t-34400's QuestMediaProjection repo](https://github.com/t-34400/QuestMediaProjection) demonstrated using Google's ML barcode reader.

[@Gustorvo](https://github.com/Gustorvo)'s pull request replaced a texture copy over the JNI with a pointer 

## Setup

- Add the 'DisplayCapture' and 'DepthKit' folders to your project.

![Screenshot_1](https://github.com/user-attachments/assets/bf96301b-badf-42fb-a05f-1da018dd33e3)

- Open your player settings and set your Android Target API level to `Android 14.0 (API level 34)`

![image](https://github.com/user-attachments/assets/98791394-e4fa-433d-bac2-c23b30a090a5)

- Make sure you're using custom Main Manifest and Main Gradle Template files

![Screenshot_2](https://github.com/user-attachments/assets/31a7ff38-13dc-4f3b-9d6b-0127e2355521)

- Update your `AndroidManifest.xml` file with these lines:

```
<!--ADD THESE LINES TO YOUR MANIFEST <MANIFEST> SECTION!!!-->
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE_MEDIA_PROJECTION" />
<!--ADD THESE LINES TO YOUR MANIFEST <MANIFEST> SECTION!!!-->
```

```
<!--ADD THESE LINES TO YOUR MANIFEST <APPLICATION> SECTION!!!-->
<activity android:name="com.trev3d.DisplayCapture.DisplayCaptureRequestActivity" android:exported="false" />
<service android:name="com.trev3d.DisplayCapture.DisplayCaptureNotificationService" android:exported="false" android:foregroundServiceType="mediaProjection" />
<!--ADD THESE LINES TO YOUR MANIFEST <APPLICATION> SECTION!!!-->
```

![image](https://github.com/user-attachments/assets/55c56c9a-8f6f-476d-b8b4-4446b51e6db1)

- Update your `mainTemplate.gradle` file with these lines:

```
/* ADD THESE LINES TO YOUR GRADLE DEPENDENCIES SECTION */
implementation 'androidx.appcompat:appcompat:1.6.1'
implementation 'com.google.mlkit:barcode-scanning:17.3.0'
implementation 'com.google.code.gson:gson:2.11.0'
/* ADD THESE LINES TO YOUR GRADLE DEPENDENCIES SECTION */
```

![image](https://github.com/user-attachments/assets/c40f34b3-de5c-4fa1-a472-842115bc7062)

- Refer to `Demo.scene` on setting up the necessary components. I'll update and document these in a future commit (sorry)

![image](https://github.com/user-attachments/assets/e7167678-b36e-44e6-86ae-774b7ab714c2)


## ⚠️ Issues (please read)!

### To fix 

⚠️ MediaProjection stop callback doesn't seem to work correctly

### Gotchas

⚠️⚠️⚠️ Google's ML barcode scanner annoyingly reorders corner point order so that codes always face 'up' relative to the viewer. This makes it near impossible to properly track the orientation of flat-facing codes as the orientation will always face 'toward' you. You can get around this by using two codes and the vector between them as your orientation

⚠️ While display capture and QR code reading will work on any headset, QR code *tracking* will only work on Quest 3 / Quest 3S due to other headsets lacking depth estimation features.

⚠️ Display capture is expensive, as is QR code tracking

⚠️ You may need to be on Quest system software v68 or higher

⚠️ This only works on-headset. This will not work through QuestLink

⚠️ You cannot video record the display 'normally' while this app's MediaProjector session is running. You can instead use [scrcpy](https://github.com/Genymobile/scrcpy) to record any prototypes or demos you make with this.

⚠️ This still isn't proper camera access. Any virtual elements will obscure physical objects in the image. If you need to track something, you must not render anything on top of it!

### Other info

- The captured view is ~82 degrees in horizontal and vertical FOV on Quest 3
- The capture texture is 1024x1024
- MediaProjection currently captures frames from the left eye buffer
- Quest system camera settings do not affect the capture resolution, framerate, or eye

## Reference

- [https://developer.oculus.com/documentation/native/native-media-projection/](https://developer.oculus.com/documentation/native/native-media-projection/)
- [https://developer.android.com/media/grow/media-projection](https://developer.android.com/media/grow/media-projection)
- [https://github.com/android/media-samples/tree/main/ScreenCapture](https://github.com/android/media-samples/tree/main/ScreenCapture)
- [https://developers.google.com/android/reference/com/google/mlkit/vision/barcode/common/Barcode](https://developers.google.com/android/reference/com/google/mlkit/vision/barcode/common/Barcode)
