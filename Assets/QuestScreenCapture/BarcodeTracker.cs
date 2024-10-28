using Anaglyph.XRTemplate.DepthKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Anaglyph.DisplayCapture
{
	public class BarcodeTracker : MonoBehaviour
	{
		[SerializeField] private BarcodeReader barcodeReader;

		[SerializeField] private float horizontalFieldOfViewDegrees = 82f;
		public float Fov => horizontalFieldOfViewDegrees;

		[SerializeField] private Transform[] indicators;

		private Matrix4x4 displayCaptureProjection;
		private new Camera camera;

		private void Awake()
		{
			barcodeReader.OnTrackBarcodes += OnTrackBarcodes;

			Vector2Int size = DisplayCaptureManager.Instance.Size;
			float aspect = size.x / (float)size.y;

			displayCaptureProjection = Matrix4x4.Perspective(Fov, aspect, 1, 100f);
			camera = Camera.main;
		}

		private void OnDestroy()
		{
			barcodeReader.OnTrackBarcodes -= OnTrackBarcodes;
		}

		private void OnTrackBarcodes(IEnumerable<BarcodeReader.Result> results)
		{

			foreach (BarcodeReader.Result result in results)
			{
				float timestampInSeconds = result.timestamp * 0.000000001f;
				OVRPlugin.PoseStatef headPoseState = OVRPlugin.GetNodePoseStateAtTime(timestampInSeconds, OVRPlugin.Node.Head);
				OVRPose headPose = headPoseState.Pose.ToOVRPose();
				Matrix4x4 headTransform = Matrix4x4.TRS(headPose.position, headPose.orientation, Vector3.one);

				Vector2[] screenPoints = new Vector2[4];

				for (int i = 0; i < 4; i++)
				{
					BarcodeReader.Point p = result.points[i];

					Vector2Int size = DisplayCaptureManager.Instance.Size;
					Vector3 toWorld = headTransform.MultiplyPoint(-Unproject(displayCaptureProjection, new Vector2(1f - p.x / (float)size.x, p.y / (float)size.y)));
					//indicators[i].position = toWorld;
					screenPoints[i] = camera.WorldToViewportPoint(toWorld, Camera.MonoOrStereoscopicEye.Left);
					//screenPoints[i].x *= 1.5f;
				}

				DepthToWorld.Sample(screenPoints, out Vector3[] worldPoints);

				for (int i = 0; i < 4; i++)
					indicators[i].position = worldPoints[i];

				break;
			}
		}

		private static Vector3 Project(Matrix4x4 projection, Vector3 )
		{
			float[] inn = new float[4];

			inn[0] = 2.0f * uv.x - 1.0f;
			inn[1] = 2.0f * uv.y - 1.0f;
			inn[2] = 0.1f;
			inn[3] = 1.0f;

			Vector4 pos = projection.inverse * new Vector4(inn[0], inn[1], inn[2], inn[3]);

			pos.w = 1.0f / pos.w;

			pos.x *= pos.w;
			pos.y *= pos.w;
			pos.z *= pos.w;

			return new Vector3(pos.x, pos.y, pos.z);
		}

		private static Vector3 Unproject(Matrix4x4 projection, Vector2 uv)
		{

			var h = new Vector4(2f * uv.x - 1f, 2f * uv.y - 1f, 0.1f, 1f);

			Vector4 pos = projection.inverse * h;

			pos.w = 1.0f / pos.w;

			pos.x *= pos.w;
			pos.y *= pos.w;
			pos.z *= pos.w;

			return new Vector3(pos.x, pos.y, pos.z);
		}
	}
}