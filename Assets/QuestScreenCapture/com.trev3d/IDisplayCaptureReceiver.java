package com.trev3d;

import android.media.Image;
import java.nio.ByteBuffer;

public interface IDisplayCaptureReceiver {
	public void onNewImage(ByteBuffer byteBuffer, int width, int height, long timestamp);
}