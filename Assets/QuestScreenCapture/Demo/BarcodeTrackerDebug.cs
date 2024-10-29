using System.Collections.Generic;
using UnityEngine;

namespace Anaglyph.DisplayCapture
{
	public class BarcodeTrackerDebug : MonoBehaviour
	{
		[SerializeField] private BarcodeTracker barcodeTracker;
		[SerializeField] private GameObject indicatorPrefab;

		private List<BarcodeIndicator> indicators = new(5);

		private void InstantiateIndicator() => indicators.Add(Instantiate(indicatorPrefab).GetComponent<BarcodeIndicator>());

		private void Awake ()
		{
			for (int i = 0; i < indicators.Capacity; i++)
				InstantiateIndicator();

			barcodeTracker.OnTrackBarcodes += OnTrackBarcodes;
		}

		private void OnDestroy()
		{
			foreach(BarcodeIndicator indicator in indicators)
			{
				Destroy(indicator.gameObject);
			}

			if(barcodeTracker != null)
				barcodeTracker.OnTrackBarcodes -= OnTrackBarcodes;
		}

		private void OnTrackBarcodes(IEnumerable<BarcodeTracker.Result> results)
		{
			int i = 0;
			foreach (BarcodeTracker.Result result in results)
			{
				if (i > indicators.Count)
					InstantiateIndicator();

				indicators[i].gameObject.SetActive(true);

				indicators[i].Set(result);
				i++;
			}

			while (i < indicators.Count)
			{
				indicators[i].gameObject.SetActive(false);
				i++;
			}
		}
	}
}