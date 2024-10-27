package com.trev3d;

import static android.content.ContentValues.TAG;

import android.graphics.Bitmap;

import com.google.gson.Gson;
import com.google.mlkit.vision.barcode.BarcodeScanner;
import com.google.mlkit.vision.barcode.BarcodeScannerOptions;
import com.google.mlkit.vision.barcode.BarcodeScanning;
import com.google.mlkit.vision.barcode.common.Barcode;
import com.google.mlkit.vision.common.InputImage;
import com.unity3d.player.UnityPlayer;

import java.io.Serializable;
import java.nio.ByteBuffer;
import java.util.Objects;

import android.util.Log;

public class BarcodeReader implements IDisplayCaptureReceiver {

	private static class Results implements Serializable {
		public Result[] results;

		public Results(int size) {
			results = new Result[size];
		}
	}

	private static class Result implements Serializable {
		public String text;
		public Point[] points;
		public long timestamp;
	}

	private static class Point implements Serializable {
		public float x, y;

		public Point(android.graphics.Point point) {
			x = point.x;
			y = point.y;
		}
	}

	public static com.trev3d.BarcodeReader instance = null;

	private final BarcodeScanner scanner;
	private final Gson gson;

	private UnityInterface unityInterface;

	private record UnityInterface(String gameObjectName) {
		private void Call(String functionName) {
			UnityPlayer.UnitySendMessage(gameObjectName, functionName, "");
		}

		public void OnBarcodeResults(String json) {
			UnityPlayer.UnitySendMessage(gameObjectName, "OnBarcodeResults", json);
		}
	}

	public BarcodeReader() {

		var optBuilder = new BarcodeScannerOptions.Builder();
		optBuilder.setBarcodeFormats(Barcode.FORMAT_QR_CODE);
		optBuilder.build();

		scanner = BarcodeScanning.getClient(optBuilder.build());

		gson = new Gson();

		DisplayCaptureManager.getInstance().receivers.add(this);
	}

	public static synchronized com.trev3d.BarcodeReader getInstance()
	{
		if (instance == null)
			instance = new com.trev3d.BarcodeReader();

		return instance;
	}

	@Override
	public void onNewImage(ByteBuffer byteBuffer, int width, int height, long timestamp) {

		var bitmap = Bitmap.createBitmap(
				width,
				height,
				Bitmap.Config.ARGB_8888
		);

		byteBuffer.rewind();

		bitmap.copyPixelsFromBuffer(byteBuffer);

		InputImage input = InputImage.fromBitmap(bitmap, 0);

		scanner.process(input).addOnCompleteListener(task -> {

			if (!task.isSuccessful()) {
				Log.v(TAG, "No barcode found.");
				return;
			}

			var taskResult = task.getResult();
			Results results = new Results(taskResult.size());

			Log.i(TAG, taskResult.size() + " barcodes found.");

			for(int i = 0; i < taskResult.size(); i++) {
				Barcode barcode = taskResult.get(i);
				Log.i(TAG, "Barcode: " + barcode.getDisplayValue());

				Result result = new Result();
				result.text = barcode.getDisplayValue();
				result.timestamp = timestamp;

				var cornerPoints = Objects.requireNonNull(barcode.getCornerPoints());
				result.points = new Point[cornerPoints.length];
				for(int j = 0; j < cornerPoints.length; j++)
					result.points[j] = new Point(cornerPoints[j]);
				results.results[i] = result;
			}

			String resultsAsJson = gson.toJson(results);
			Log.i(TAG, "JSON: " + resultsAsJson);
			unityInterface.OnBarcodeResults(resultsAsJson);
		});
	}

	// called by Unity
	public void setup(String gameObjectName) {
		unityInterface = new UnityInterface(gameObjectName);
	}

//	public BarcodeResult[] getResults() {
//		return barcodeResults;
//	}
}
