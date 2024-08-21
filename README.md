# Quest Display Access Demo

Developers want camera access on the Meta Quest. Meta hasn't let us have it yet. The next best thing is display access. Thanks to Android's MediaProjector API, you can copy the display image to a texture in your Unity project with minimal latency as demonstrated in this horribly architected Unity project. **No PC or embedded browser required!**

# ⚠️⚠️⚠️  Issues (please read)!

⚠️ (To fix...) **The project assumes your Quest camera resolution setting is set to 1024x1024.** Go into your Quest camera settings and set this if you haven't already.

⚠️ (To fix...) The app only creates a single MediaProjector session on app launch. If the app is interrupted (such as by the headset going to sleep) the session will end and you'll need to restart the app. 

⚠️ You cannot video record the display 'normally' while this app's MediaProjector session is running. You can instead use [scrcpy](https://github.com/Genymobile/scrcpy) to record any prototypes or demos you make with this.

⚠️ (To fix...) The code for copying the texture to Unity is horribly unoptimized due to my lack of experience with Android's APIs. 

⚠️ This still isn't proper camera access. Any virtual elements will obscure physical objects in the image. If you need to track something, you must not render anything on top of it!
