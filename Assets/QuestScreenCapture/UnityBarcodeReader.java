package com.trev3d;

import static android.content.ContentValues.TAG;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.ImageFormat;
import android.graphics.PixelFormat;
import android.media.Image;

import com.google.mlkit.vision.barcode.BarcodeScanner;
import com.google.mlkit.vision.barcode.BarcodeScannerOptions;
import com.google.mlkit.vision.barcode.BarcodeScanning;
import com.google.mlkit.vision.barcode.common.Barcode;
import com.google.mlkit.vision.common.InputImage;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import android.util.Log;

import com.trev3d.BarcodeResult;
import com.unity3d.player.UnityPlayer;

public class UnityBarcodeReader implements com.trev3d.IMediaProjectionReceiver {

	private final BarcodeScanner scanner;

	private BarcodeResult[] barcodeResults;

	public static com.trev3d.UnityBarcodeReader instance = null;
	public static synchronized com.trev3d.UnityBarcodeReader getInstance()
	{
		if (instance == null)
			instance = new com.trev3d.UnityBarcodeReader();

		return instance;
	}

	private String gameObjectName;

	public void setup(String gameObjectName) {
		this.gameObjectName = gameObjectName;
	}

	private Bitmap convertToBitmap(Image image) {
		var planes = image.getPlanes();

		var buffer = planes[0].getBuffer();

		var pixelStride = planes[0].getPixelStride();
		var rowStride = planes[0].getRowStride();
		var rowPadding = rowStride - pixelStride * image.getWidth();


		var bitmap = Bitmap.createBitmap(
				image.getWidth() + rowPadding / pixelStride,
				image.getHeight(),
				Bitmap.Config.ARGB_8888
		);

		bitmap.copyPixelsFromBuffer(buffer);

		return bitmap;
	}

	public UnityBarcodeReader() {
		var optBuilder = new BarcodeScannerOptions.Builder();
		optBuilder.setBarcodeFormats(Barcode.FORMAT_QR_CODE);
		optBuilder.build();

		scanner = BarcodeScanning.getClient(optBuilder.build());

		com.trev3d.UnityMediaProjection.getInstance().receivers.add(this);
	}

	@Override
	public void onNewImage(Image image) {

		Bitmap bitmap = convertToBitmap(image);
		InputImage input = InputImage.fromBitmap(bitmap, 0);

		scanner.process(input).addOnCompleteListener(task -> {

			if (task.isSuccessful()) {

				ArrayList<BarcodeResult> results = new ArrayList<>();

				var taskResult = task.getResult();

				Log.i(TAG, taskResult.size() + " barcodes found.");

				for(int i = 0; i < taskResult.size(); i++) {
					Barcode barcode = taskResult.get(i);
					Log.i(TAG, "Barcode: " + barcode.getDisplayValue());

					BarcodeResult result = new BarcodeResult();
					result.text = barcode.getDisplayValue();
					result.format = barcode.getFormat();
//					result.points = barcode.getCornerPoints();

					results.add(result);
				}

				// barcodeResults = results.toArray(barcodeResults);

//				UnityPlayer.UnitySendMessage(gameObjectName, "OnNewBarcodeResults", "");

			} else {
				Log.v(TAG, "No barcode found.");
			}
		});
	}

	public BarcodeResult[] getResults() {
		return barcodeResults;
	}
}
