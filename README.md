# Quest Display Access Demo

Developers want camera access on the Meta Quest. Meta hasn't let us have it yet. The next best thing is display access. Thanks to Android's MediaProjector API, you can copy the display image to a texture in your Unity project with minimal latency as demonstrated in this horribly architected Unity project. **No PC or embedded browser required!**

I spent several days banging stones together with zero Android experience prior to this. If you find this helpful I'd appreciate a mention of my [website trev3d.com](https://trev3d.com) or [Twitter @trev3d](https://twitter.com/trev3d) for the time :)

# ⚠️⚠️⚠️  Important! Please please read this! ⚠️⚠️⚠️

While this project is functional, there are many problems I need to fix...

⚠️ **This version of the project has hard-coded values that expect your Quest camera resolution setting to be at 1024x1024.**
Go into your Quest camera settings and set this if you haven't already.

⚠️ You cannot video record the display 'normally' while this app's MediaProjector session is running. You can instead use [scrcpy](https://github.com/Genymobile/scrcpy) to record any prototypes or demos you make with this.

⚠️ A single MediaProjector session only runs once on app launch. If interrupted (such as by the headset going to sleep) the session will not restart until you relaunch the app. I'll fix this soon :)

⚠️ Please don't use this for anything more than prototyping. I'm not an Android developer nor do I know its best practices. **I do know that copying an entire texture from the GPU into a byte array on the CPU every frame, and then copying that byte array into a Unity Texture2D, and then sending that back to the GPU is a bad bad idea.** Unfortunately due to my Android skill issue I don't immediately know of a better way to do this yet.

⚠️ This still isn't proper camera access. Any virtual elements will obscure physical objects in the image. If you need to track something, you must not render anything on top of it!
