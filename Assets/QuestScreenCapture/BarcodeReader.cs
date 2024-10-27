using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anaglyph.DisplayCapture
{
	public class BarcodeReader : MonoBehaviour
	{
		[Serializable]
		private struct Results
		{
			public Result[] results;
		}

		[Serializable]
		public struct Result
		{
			public string text;
			public Point[] points;
			public long timestamp;
		}

		[Serializable]
		public struct Point
		{
			public float x, y;
		}

		private class AndroidInterface
		{
			private AndroidJavaClass androidClass;
			private AndroidJavaObject androidInstance;

			public AndroidInterface(GameObject messageReceiver)
			{
				androidClass = new AndroidJavaClass("com.trev3d.BarcodeReader");
				androidInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
				androidInstance.Call("setup", messageReceiver.name);
			}
		}

		public event Action<IEnumerable<Result>> OnTrackBarcodes = delegate { };

		private AndroidInterface androidInterface;

		private void Start()
		{
			androidInterface = new AndroidInterface(gameObject);
		}

		private void OnBarcodeResults(string json)
		{
			Results results = JsonUtility.FromJson<Results>(json);
			OnTrackBarcodes.Invoke(results.results);
		}
	}
}