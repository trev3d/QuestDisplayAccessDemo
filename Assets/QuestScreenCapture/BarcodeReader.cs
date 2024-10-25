using System;
using UnityEngine;

public class BarcodeReader : MonoBehaviour
{
	private AndroidJavaClass BarcodeReaderClass;
	private AndroidJavaObject BarcodeReaderInstance;

	[Serializable]
	private struct BarcodeResult
	{
		public string text;
		public int format;
		public long timestamp;
	}

	private void Start()
	{
		BarcodeReaderClass = new AndroidJavaClass("com.trev3d.UnityBarcodeReader");
		BarcodeReaderInstance = BarcodeReaderClass.CallStatic<AndroidJavaObject>("getInstance");
		BarcodeReaderInstance.Call("setup", gameObject.name);
	}
}
