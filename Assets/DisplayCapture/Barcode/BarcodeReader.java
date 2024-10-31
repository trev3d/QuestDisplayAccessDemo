package com.trev3d.DisplayCapture;

import static android.content.ContentValues.TAG;

import android.graphics.Bitmap;

import com.google.android.gms.tasks.Task;
import com.google.gson.Gson;
import com.google.mlkit.vision.barcode.BarcodeScanner;
import com.google.mlkit.vision.barcode.BarcodeScannerOptions;
import com.google.mlkit.vision.barcode.BarcodeScanning;
import com.google.mlkit.vision.barcode.common.Barcode;
import com.google.mlkit.vision.common.InputImage;
import com.unity3d.player.UnityPlayer;

import java.io.Serializable;
import java.nio.ByteBuffer;
import java.util.List;
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

	public static BarcodeReader instance = null;

	private final BarcodeScanner scanner;
	private final Gson gson;
	
	private boolean enabled;
	private volatile boolean readingBarcode = false;

	private UnityInterface unityInterface;

	private static class UnityInterface {
		private final String gameObjectName;

		private UnityInterface(String gameObjectName) {
			this.gameObjectName = gameObjectName;
		}

		private void Call(String functionName) {
			UnityPlayer.UnitySendMessage(gameObjectName, functionName, "");
		}

		public void OnBarcodeResults(String json) {
			UnityPlayer.UnitySendMessage(gameObjectName, "OnBarcodeResults", json);
		}
	}

	public BarcodeReader() {
		BarcodeScannerOptions.Builder optBuilder = new BarcodeScannerOptions.Builder();
		optBuilder.setBarcodeFormats(Barcode.FORMAT_QR_CODE);
		optBuilder.build();

		scanner = BarcodeScanning.getClient(optBuilder.build());
		gson = new Gson();
	}

	public static synchronized BarcodeReader getInstance()
	{
		if (instance == null)
			instance = new BarcodeReader();

		return instance;
	}

	public void setEnabled(boolean enabled) {
		if(this.enabled == enabled)
			return;

		this.enabled = enabled;

		if(this.enabled) {
			DisplayCaptureManager.getInstance().receivers.add(this);
		} else {
			DisplayCaptureManager.getInstance().receivers.remove(this);
		}
	}

	@Override
	public void onNewImage(ByteBuffer byteBuffer, int width, int height, long timestamp) {

		if(readingBarcode)
			return;

		Bitmap bitmap = Bitmap.createBitmap(
				width,
				height,
				Bitmap.Config.ARGB_8888
		);

		byteBuffer.rewind();
		bitmap.copyPixelsFromBuffer(byteBuffer);

		InputImage input = InputImage.fromBitmap(bitmap, 0);

		readingBarcode = true;
		scanner.process(input).addOnCompleteListener(task -> {

			readingBarcode = false;

			if (!task.isSuccessful()) {
				Log.v(TAG, "No barcode found.");
				return;
			}

			List<Barcode> taskResult = task.getResult();
			Results results = new Results(taskResult.size());

			Log.i(TAG, taskResult.size() + " barcodes found.");

			for(int i = 0; i < taskResult.size(); i++) {
				Barcode barcode = taskResult.get(i);
				Log.i(TAG, "Barcode: " + barcode.getDisplayValue());

				Result result = new Result();
				result.text = barcode.getDisplayValue();
				result.timestamp = timestamp;

				android.graphics.Point[] cornerPoints = Objects.requireNonNull(barcode.getCornerPoints());
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
