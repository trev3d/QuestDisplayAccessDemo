package com.trev3d;

import static android.content.ContentValues.TAG;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.SurfaceTexture;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.Image;
import android.media.ImageReader;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.opengl.EGL14;
import android.opengl.EGLConfig;
import android.opengl.EGLContext;
import android.opengl.EGLDisplay;
import android.opengl.EGLSurface;
import android.opengl.GLES11Ext;
import android.opengl.GLES20;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.util.Log;
import android.view.Surface;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.nio.ByteBuffer;
import java.util.Objects;

public class UnityPlayerActivityWithMediaProjector extends UnityPlayerActivity
{
	private static final String GAME_OBJECT_NAME = "DisplayReceiver";
	private static final int REQUEST_MEDIA_PROJECTION = 1;
	private MediaProjection projection;
	private SurfaceTexture surfaceTexture;
	private Surface surface;
	private VirtualDisplay virtualDisplay;

	public static int textureId;

    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
		var manager = (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);
		startActivityForResult(manager.createScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);
    }

	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_MEDIA_PROJECTION) {
			if (resultCode != Activity.RESULT_OK) {
				Log.i(TAG, "User declined screen capture");
				return;
			}

			Intent intent = new Intent(this, com.trev3d.RecordNotificationService.class);

			intent.putExtra("code", resultCode);
			intent.putExtra("data", data);

			startService(intent);

			EGLDisplay eglDisplay = EGL14.eglGetDisplay(EGL14.EGL_DEFAULT_DISPLAY);
			if (eglDisplay == EGL14.EGL_NO_DISPLAY) {
				throw new RuntimeException("Unable to get EGL14 display");
			}

			int[] version = new int[2];
			if (!EGL14.eglInitialize(eglDisplay, version, 0, version, 1)) {
				throw new RuntimeException("Unable to initialize EGL14");
			}

			int[] attribList = {
					EGL14.EGL_RED_SIZE, 8,
					EGL14.EGL_GREEN_SIZE, 8,
					EGL14.EGL_BLUE_SIZE, 8,
					EGL14.EGL_ALPHA_SIZE, 8,
					EGL14.EGL_RENDERABLE_TYPE, EGL14.EGL_OPENGL_ES2_BIT,
					EGL14.EGL_NONE
			};

			EGLConfig[] configs = new EGLConfig[1];
			int[] numConfigs = new int[1];
			if (!EGL14.eglChooseConfig(eglDisplay, attribList, 0, configs, 0, configs.length, numConfigs, 0)) {
				throw new IllegalArgumentException("Failed to choose EGL config");
			}
			EGLConfig eglConfig = configs[0];

			int[] attrib_list = {
					EGL14.EGL_CONTEXT_CLIENT_VERSION, 2, // OpenGL ES 2.0
					EGL14.EGL_NONE
			};

			EGLContext eglContext = EGL14.eglCreateContext(eglDisplay, eglConfig, EGL14.EGL_NO_CONTEXT, attrib_list, 0);
			if (eglContext == null || eglContext == EGL14.EGL_NO_CONTEXT) {
				throw new RuntimeException("Failed to create EGL context");
			}

			int[] surfaceAttribs = {
					EGL14.EGL_NONE
			};
			EGLSurface eglSurface = EGL14.eglCreatePbufferSurface(eglDisplay, eglConfig, surfaceAttribs, 0);
			if (eglSurface == null || eglSurface == EGL14.EGL_NO_SURFACE) {
				throw new RuntimeException("Failed to create EGL surface");
			}

			if (!EGL14.eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext)) {
				throw new RuntimeException("Failed to make EGL context current");
			}

			final int width = 1024;
			final int height = 1024;

			int[] textures = new int[1];
			GLES20.glGenTextures(1, textures, 0);

			textureId = textures[0];

			GLES20.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, textureId);

			// Set texture parameters
			GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
			GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
			GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
			GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);

			surfaceTexture = new SurfaceTexture(textureId);
			surface = new Surface(surfaceTexture);

			UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, "InitializeExternalTexture", String.valueOf(textureId));

			surfaceTexture.setOnFrameAvailableListener(new SurfaceTexture.OnFrameAvailableListener() {
				@Override
				public void onFrameAvailable(SurfaceTexture surfaceTexture) {
					UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, "OnFrameAvailable", "");
				}
			});

			new Handler(Looper.getMainLooper()).postDelayed(new Runnable() {
				@Override
				public void run() {

					projection = ((MediaProjectionManager) Objects.requireNonNull(getSystemService(Context.MEDIA_PROJECTION_SERVICE))).
							getMediaProjection(resultCode, data);

					virtualDisplay = projection.createVirtualDisplay("ScreenCapture",
							width, height, 300,
							DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR,
							surface, null, null);
				}
			}, 1000);
		}
	}
}
