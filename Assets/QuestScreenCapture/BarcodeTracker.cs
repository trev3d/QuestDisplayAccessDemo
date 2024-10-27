using System.Collections.Generic;
using UnityEngine;

namespace Anaglyph.DisplayCapture
{
	public class BarcodeTracker : MonoBehaviour
	{
		[SerializeField] private BarcodeReader barcodeReader;

		[SerializeField] private Vector2Int imageSize;
		[SerializeField] private float fieldOfView = 82f;
		private float focalLength;

		[SerializeField] private Transform testTransform;

		private Transform cameraTransform;

		private void Awake()
		{
			barcodeReader.OnTrackBarcodes += OnTrackBarcodes;
			cameraTransform = Camera.main.transform;
		}

		private void OnDestroy()
		{
			barcodeReader.OnTrackBarcodes -= OnTrackBarcodes;
		}

		private void OnTrackBarcodes(IEnumerable<BarcodeReader.Result> results)
		{

			foreach (BarcodeReader.Result result in results)
			{
				
			}
		}
	}
}