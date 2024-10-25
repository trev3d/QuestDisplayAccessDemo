package com.trev3d;

import static android.content.ContentValues.TAG;

import android.content.Context;
import android.content.Intent;
import android.graphics.PixelFormat;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.Image;
import android.media.ImageReader;
import android.media.projection.MediaProjection;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import com.trev3d.IMediaProjectionReceiver;

import com.unity3d.player.UnityPlayer;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Objects;

public class UnityMediaProjection {

	private ImageReader reader;
	private MediaProjection projection;
	private VirtualDisplay virtualDisplay;
	private Intent notifServiceIntent;

	private ByteBuffer lastFrameBytesBuffer;

	private String gameObjectName;
	private int width;
	private int height;

	public ArrayList<IMediaProjectionReceiver> receivers;

	public static UnityMediaProjection instance = null;
	public static synchronized UnityMediaProjection getInstance()
	{
		if (instance == null)
			instance = new UnityMediaProjection();

		return instance;
	}

	public UnityMediaProjection() {
		receivers = new ArrayList<>();
	}

	private void SendUnityMessage(String methodName) {
		UnityPlayer.UnitySendMessage(gameObjectName, methodName, "");
	}

	public void initialize(String gameObjectName, int width, int height) {

		this.gameObjectName = gameObjectName;
		this.width = width;
		this.height = height;

		// Calculate the exact buffer size required (4 bytes per pixel for RGBA_8888)
		int bufferSize = width * height * 4;

		// Allocate a direct ByteBuffer for better performance
		lastFrameBytesBuffer = ByteBuffer.allocateDirect(bufferSize);

		reader = ImageReader.newInstance(width, height, PixelFormat.RGBA_8888, 2);

		reader.setOnImageAvailableListener(imageReader -> {

			SendUnityMessage("NewFrameIncoming");

			Image image = imageReader.acquireLatestImage();

			if (image != null) {
				for(int i = 0; i < receivers.size(); i++)
					receivers.get(i).onNewImage(image);

				ByteBuffer buffer = image.getPlanes()[0].getBuffer();

				// Clear the buffer for new data by resetting the position of the buffer to zero
				lastFrameBytesBuffer.clear();
				lastFrameBytesBuffer.put(buffer);

				image.close();

				SendUnityMessage("NewFrameAvailable");
			}

		}, new Handler(Looper.getMainLooper()));
	}

	public void requestScreenCapture() {
		Log.i(TAG, "Asking for screen capture permission...");
		Intent intent = new Intent(UnityPlayer.currentActivity, com.trev3d.UnityMediaProjectionRequestActivity.class);
		UnityPlayer.currentActivity.startActivity(intent);
	}

	public void onGetScreenCapturePermission(int resultCode, Intent intent) {

		notifServiceIntent = new Intent(UnityPlayer.currentActivity, com.trev3d.UnityMediaProjectionNotificationService.class);
		UnityPlayer.currentActivity.startService(notifServiceIntent);

		new Handler(Looper.getMainLooper()).postDelayed(() -> {

			Log.i(TAG, "Starting screen capture...");

			projection = ((android.media.projection.MediaProjectionManager)
					Objects.requireNonNull(UnityPlayer.currentActivity.getSystemService(Context.MEDIA_PROJECTION_SERVICE))).
					getMediaProjection(resultCode, intent);

			projection.registerCallback(new MediaProjection.Callback() {
				@Override
				public void onStop() {

					Log.i(TAG, "Screen capture ended!");

					handleScreenCaptureEnd();
				}
			}, new Handler(Looper.getMainLooper()));

			virtualDisplay = projection.createVirtualDisplay("ScreenCapture",
					width, height, 300,
					DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR,
					reader.getSurface(), null, null);

			SendUnityMessage("ScreenCaptureStarted");

		}, 100);

		Log.i(TAG, "Screen capture started!");
	}

	public void stopScreenCapture() {
		Log.i(TAG, "Stopping screen capture...");

		if(projection == null) return;
		projection.stop();
	}

	private void handleScreenCaptureEnd() {

		virtualDisplay.release();
		UnityPlayer.currentActivity.stopService(notifServiceIntent);

		SendUnityMessage("ScreenCaptureStopped");
	}

	public ByteBuffer getLastFrameBytesBuffer() {
		return lastFrameBytesBuffer;
	}
}
