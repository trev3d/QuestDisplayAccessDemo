package com.trev3d;

import static android.content.ContentValues.TAG;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.SurfaceTexture;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.opengl.EGL14;
import android.opengl.EGLContext;
import android.opengl.EGLDisplay;
import android.opengl.EGLSurface;
import android.opengl.GLES11Ext;
import android.opengl.GLES30;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.Surface;

import com.unity3d.player.UnityPlayerActivity;

import java.util.Objects;

public class UnityPlayerActivityWithMediaProjector extends UnityPlayerActivity implements SurfaceTexture.OnFrameAvailableListener
{
	private static final int REQUEST_MEDIA_PROJECTION = 1;
	private MediaProjection projection;
	private VirtualDisplay virtualDisplay;
	private SurfaceTexture surfaceTexture;
	private Surface surface;

	int resultCode;
	Intent resultData;

	private int width;
	private int height;

	private static EGLContext unityContext = EGL14.EGL_NO_CONTEXT;
	private static EGLDisplay unityDisplay = EGL14.EGL_NO_DISPLAY;
	private static EGLSurface unityDrawSurface = EGL14.EGL_NO_SURFACE;
	private static EGLSurface unityReadSurface = EGL14.EGL_NO_SURFACE;

	// https://medium.com/xrpractices/external-texture-rendering-with-unity-and-android-b844bb7a35da

	public void initSurface(int textureId, int width, int height) {

		if(!Thread.currentThread().getName().equals("UnityMain")) {
			Log.e(TAG, "Cannot init surface. Not called from render thread.");
			return;
		}

		this.width = width;
		this.height = height;

		unityContext = EGL14.eglGetCurrentContext();
		unityDisplay = EGL14.eglGetCurrentDisplay();
		unityDrawSurface = EGL14.eglGetCurrentSurface(EGL14.EGL_DRAW);
		unityReadSurface = EGL14.eglGetCurrentSurface(EGL14.EGL_READ);

		if (unityContext == EGL14.EGL_NO_CONTEXT) {
			Log.e(TAG, "UnityEGLContext is invalid -> Most probably wrong thread");
		}

		Log.i(TAG, "Setting up surface texture with id " + textureId + " and dimensions " + width + "x" + height);

		EGL14.eglMakeCurrent(unityDisplay, unityDrawSurface, unityReadSurface, unityContext);

		GLES30.glActiveTexture(GLES30.GL_TEXTURE0);
		GLES30.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, textureId);
		GLES30.glTexParameterf(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES30.GL_TEXTURE_MIN_FILTER, GLES30.GL_LINEAR);
		GLES30.glTexParameterf(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES30.GL_TEXTURE_MAG_FILTER, GLES30.GL_LINEAR);
		GLES30.glTexParameterf(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES30.GL_TEXTURE_WRAP_S, GLES30.GL_CLAMP_TO_EDGE);
		GLES30.glTexParameterf(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, GLES30.GL_TEXTURE_WRAP_T, GLES30.GL_CLAMP_TO_EDGE);

		surfaceTexture = new SurfaceTexture(textureId);
		surfaceTexture.setDefaultBufferSize(width, height);

		surface = new Surface(surfaceTexture);
		surfaceTexture.setOnFrameAvailableListener(this);

		Log.i(TAG, "Texture should be set up!");
	}

	public void requestScreenCapturePermissionAndStart() {
		var manager = (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);
		startActivityForResult(manager.createScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);
	}

	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_MEDIA_PROJECTION) {
			if (resultCode != Activity.RESULT_OK) {
				Log.i(TAG, "User declined screen capture. Asking again");

				requestScreenCapturePermissionAndStart();
				return;
			}

			this.resultCode = resultCode;
			this.resultData = data;
		}

		startScreenCapture();
	}

	public void startScreenCapture() {

		if(surface == null) return;

		Log.i(TAG, "starting screen capture");

		Intent intent = new Intent(this, com.trev3d.RecordNotificationService.class);
		startService(intent);

		new Handler(Looper.getMainLooper()).postDelayed(() -> {

			var manager = (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);
			projection = manager.getMediaProjection(resultCode, resultData);

			virtualDisplay = projection.createVirtualDisplay("ScreenCapture",
					width, height, 300,
					DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR,
					surface, null, null);

			Log.i(TAG, "screen capture started!");

		}, 500);
	}

	private boolean newFrameAvailable;

	public void requestSurfaceTextureUpdate() {
		if(!newFrameAvailable) return;

		if(!Thread.currentThread().getName().equals("UnityMain")) {
			Log.e(TAG, "Not called from render thread and hence update texture will fail");
			return;
		}

		Log.i(TAG, "Updating texture image");
		surfaceTexture.updateTexImage();
		newFrameAvailable = false;
	}

	@Override
	public void onFrameAvailable(SurfaceTexture surfaceTexture) {
		newFrameAvailable = true;
	}
}
