package com.trev3d;

import static android.content.ContentValues.TAG;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.PixelFormat;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.Image;
import android.media.ImageReader;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.nio.ByteBuffer;
import java.util.Objects;

public class UnityPlayerActivityWithMediaProjector extends UnityPlayerActivity {

	private static final int REQUEST_MEDIA_PROJECTION = 1;

	private ImageReader reader;
	private MediaProjection projection;
	private VirtualDisplay virtualDisplay;
	private Intent notifServiceIntent;
	
	private ByteBuffer lastFrameBytesBuffer;

	private String gameObjectName;
	private int width;
	private int height;

	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
	}

	private void SendUnityMessage(String methodName) {
		UnityPlayer.UnitySendMessage(gameObjectName, methodName, "");
	}

	public void startScreenCaptureWithPermission(String gameObjectName, int width, int height) {

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
               ByteBuffer buffer = image.getPlanes()[0].getBuffer();   
              
               // Clear the buffer for new data by resetting the position of the buffer to zero
               lastFrameBytesBuffer.clear(); 
               lastFrameBytesBuffer.put(buffer);
   
               image.close();
   
               SendUnityMessage("NewFrameAvailable");
           }
   
       }, new Handler(Looper.getMainLooper()));

		Log.i(TAG, "Asking for screen capture permission...");

		var manager = (MediaProjectionManager) getSystemService(Context.MEDIA_PROJECTION_SERVICE);
		startActivityForResult(manager.createScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);
	}

	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent intent) {
		if (requestCode != REQUEST_MEDIA_PROJECTION) return;

		if (resultCode != Activity.RESULT_OK) {
			Log.i(TAG, "User declined screen capture");

			SendUnityMessage("ScreenCapturePermissionDeclined");

			return;
		}

		Log.i(TAG, "Got screen capture permission!");

		onGetScreenCapturePermission(resultCode, intent);
	}

	private void onGetScreenCapturePermission(int resultCode, Intent intent) {

		notifServiceIntent =
				new Intent(this, com.trev3d.RecordNotificationService.class);
		startService(notifServiceIntent);

//		HandlerThread thread = new HandlerThread("Image Listener");
//		thread.start();
//		final Handler backgroudHandler = new Handler(thread.getLooper())

		new Handler(Looper.getMainLooper()).postDelayed(() -> {

			Log.i(TAG, "Starting screen capture...");

			projection = ((MediaProjectionManager) Objects.requireNonNull(getSystemService(Context.MEDIA_PROJECTION_SERVICE))).
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
		stopService(notifServiceIntent);

		SendUnityMessage("ScreenCaptureStopped");
	}
	
    public ByteBuffer getLastFrameBytesBuffer() {
        return lastFrameBytesBuffer;}
}
