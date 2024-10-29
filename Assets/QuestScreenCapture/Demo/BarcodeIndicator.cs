using Anaglyph.DisplayCapture;
using TMPro;
using UnityEngine;

public class BarcodeIndicator : MonoBehaviour
{
	[SerializeField] private LineRenderer lineRenderer;
	public LineRenderer LineRenderer => lineRenderer;

	[SerializeField] private TMP_Text textMesh;
	public TMP_Text TextMesh => textMesh;

	public void Set(BarcodeTracker.Result result) => Set(result.text, result.corners);

	public void Set(string text, Vector3[] positions)
	{
		Vector3 topCenter = (positions[0] + positions[1]) / 2f;
		transform.position = topCenter;

		Vector3 up = (positions[0] - positions[3]).normalized;
		Vector3 right = (positions[2] - positions[3]).normalized;
		Vector3 normal = -Vector3.Cross(up, right).normalized;

		transform.rotation = Quaternion.LookRotation(normal, up);

		lineRenderer.SetPositions(positions);
		textMesh.text = text;
	}
}
