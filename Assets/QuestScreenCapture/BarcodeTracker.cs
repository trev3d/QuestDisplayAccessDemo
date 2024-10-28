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

				Vector3[] worldPoints = new Vector3[4];

				for (int i = 0; i < 4; i++)
				{
					BarcodeReader.Point p = result.points[i];

					Vector2Int size = DisplayCaptureManager.Instance.Size;

					Vector2 uv = new Vector2(p.x / size.x, 1f - p.y / size.y);
					Vector3 inWorld = Unproject(displayCaptureProjection, uv);
					inWorld.z = -inWorld.z;
					inWorld = headTransform.MultiplyPoint(inWorld);
					worldPoints[i] = inWorld;

					//indicators[i].position = toWorld;
				}

				DepthToWorld.SampleWorld(worldPoints, out Vector3[] worldPoints2);

				for (int i = 0; i < 4; i++)
					indicators[i].position = worldPoints2[i];

				break;
			}
		}

		private static Vector3 Project(Matrix4x4 projection, Vector3 world)
		{
			var h = new Vector4(world.x, world.y, world.z, 1f);

			Vector4 posh = projection * h;

			return (new Vector3(posh.x, posh.y, posh.z) / posh.w + Vector3.one) / 2f;
		}

		private static Vector3 Unproject(Matrix4x4 projection, Vector2 uv)
		{
			var h = new Vector4(2f * uv.x - 1f, 2f * uv.y - 1f, 0.1f, 1f);

			Vector4 pos = projection.inverse * h;

			return new Vector3(pos.x, pos.y, pos.z) / pos.w;
		}
	}
}