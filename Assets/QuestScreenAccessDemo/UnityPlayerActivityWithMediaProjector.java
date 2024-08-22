package com.trev3d;

import static android.content.ContentValues.TAG;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.Matrix;
import android.graphics.PixelFormat;
import android.graphics.SurfaceTexture;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.Image;
import android.media.ImageReader;
import android.media.projection.MediaProjection;
import android.media.projection.MediaProjectionManager;
import android.opengl.GLES20;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.Surface;
import android.view.Window;

import com.unity3d.player.IUnityPlayerLifecycleEvents;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.nio.Buffer;
import java.nio.ByteBuffer;
import java.util.Objects;

public class UnityPlayerActivityWithMediaProjector extends UnityPlayerActivity implements ImageReader.OnImageAvailableListener
{
	private static final int REQUEST_MEDIA_PROJECTION = 1;

	private ImageReader mReader;
	private MediaProjection mMediaProjection;
	private VirtualDisplay mVirtualDisplay;

	private MediaProjectionManager mMediaProjectionManager;

	private static byte[] mLastImageBytes = null;

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

			final int width = 1024;
			final int height = 1024;

			mReader = ImageReader.newInstance(width, height, PixelFormat.RGBA_8888, 5);

			HandlerThread thread = new HandlerThread("Image Listener");
			thread.start();
			final Handler backgroudHandler = new Handler(thread.getLooper());
			mReader.setOnImageAvailableListener(this, backgroudHandler);

			int[] framebuffer = new int[1];
			int[] texture = new int[1];

			new Handler(Looper.getMainLooper()).postDelayed(new Runnable() {
				@Override
				public void run() {

					mMediaProjection = ((MediaProjectionManager) Objects.requireNonNull(getSystemService(Context.MEDIA_PROJECTION_SERVICE))).
							getMediaProjection(resultCode, data);

					mVirtualDisplay = mMediaProjection.createVirtualDisplay("ScreenCapture",
							width, height, 300,
							DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR,
							mReader.getSurface(), null, null);

				}
			}, 1000);
		}
	}

	@Override
	public void onImageAvailable(ImageReader imageReader) {

		Image image = imageReader.acquireLatestImage();

		int width = image.getWidth();
		int height = image.getHeight();

		Image.Plane plane = image.getPlanes()[0];
		ByteBuffer buffer = plane.getBuffer();
		ByteBuffer flippedBuffer = ByteBuffer.allocate(buffer.capacity());

		int pixelStride = plane.getPixelStride();
		int rowStride = plane.getRowStride();

		for (int y = 0; y < height; y++) {
			int srcRowOffset = y * rowStride;
			int dstRowOffset = (height - 1 - y) * rowStride;
			for (int x = 0; x < width; x++) {
				int srcIndex = srcRowOffset + x * pixelStride;
				int dstIndex = dstRowOffset + x * pixelStride;

				flippedBuffer.put(dstIndex, buffer.get(srcIndex));     // R
				flippedBuffer.put(dstIndex + 1, buffer.get(srcIndex + 1)); // G
				flippedBuffer.put(dstIndex + 2, buffer.get(srcIndex + 2)); // B
				flippedBuffer.put(dstIndex + 3, buffer.get(srcIndex + 3)); // A
			}
		}

		mLastImageBytes = new byte[flippedBuffer.capacity()];
		flippedBuffer.rewind();
		flippedBuffer.get(mLastImageBytes);

		UnityPlayer.UnitySendMessage("MediaProjectionTextureReceiver", "OnNewFrameAvailable", "");

		image.close();
	}

	public static byte[] getLastRecordedImage() {
		return mLastImageBytes;
	}
}
