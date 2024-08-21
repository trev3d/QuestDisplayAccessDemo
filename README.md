# Quest Display Access Demo

Developers want camera access on the Meta Quest. Meta hasn't let us have it yet. The next best thing is display access. Thanks to Android's MediaProjector API, you can access the display image in your Unity project with minimal latency as demonstrated in this horribly architected Unity project. **No PC or embedded browser required!**

# ⚠️⚠️⚠️  Important! Please please read this! ⚠️⚠️⚠️

⚠️ **This version of the project has hard-coded values that expect your Quest camera resolution setting to be at 1024x1024.**
Go into your Quest camera settings and set this if you haven't already.

⚠️ You cannot video record the display 'normally' while this app's MediaProjector session is running. You can instead use [scrcpy](https://github.com/Genymobile/scrcpy) to record any prototypes or demos you make with this.

⚠️ A single MediaProjector session only runs once on app launch. If interrupted (such as by the headset going to sleep) the session will not restart until you relaunch the app. I'll fix this soon :)

⚠️ Please don't use this for anything more than prototyping. I'm not an Android developer nor do I know its best practices. **I do know that copying an entire texture from the GPU into a byte array on the CPU every frame, and then reading that byte array into a Unity Texture2D, and then sending that back to the GPU is a bad bad idea.** Unfortunately due to my Android skill issue I don't know of a better way to do this at the moment.

⚠️ This still isn't proper camera access. Any virtual elements will obscure physical objects in the image. If you need to track something, you can't render anything on top of it!
