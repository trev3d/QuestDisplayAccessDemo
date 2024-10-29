using TMPro;
using UnityEngine;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class Indicator : MonoBehaviour
	{
		[SerializeField] private LineRenderer lineRenderer;
		public LineRenderer LineRenderer => lineRenderer;

		[SerializeField] private TMP_Text textMesh;
		public TMP_Text TextMesh => textMesh;

		private Vector3[] offsetPositions = new Vector3[4];

		public void Set(BarcodeTracker.Result result) => Set(result.text, result.corners);

		public void Set(string text, Vector3[] corners)
		{
			Vector3 topCenter = (corners[0] + corners[1]) / 2f;
			transform.position = topCenter;

			Vector3 up = (corners[0] - corners[3]).normalized;
			Vector3 right = (corners[2] - corners[3]).normalized;
			Vector3 normal = -Vector3.Cross(up, right).normalized;

			Vector3 center = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

			for (int i = 0; i < 4; i++)
			{
				Vector3 dir = (corners[i] - center).normalized;
				offsetPositions[i] = corners[i] + (dir * lineRenderer.startWidth);
			}

			transform.rotation = Quaternion.LookRotation(normal, up);

			lineRenderer.SetPositions(offsetPositions);
			textMesh.text = text;
		}
	}
}