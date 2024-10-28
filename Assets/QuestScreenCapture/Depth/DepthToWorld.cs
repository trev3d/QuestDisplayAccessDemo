using System.Runtime.InteropServices;
using UnityEngine;

namespace Anaglyph.XRTemplate.DepthKit
{
	[DefaultExecutionOrder(-30)]
	public class DepthToWorld : MonoBehaviour
	{
		private static readonly int Requests_ID = Shader.PropertyToID("Requests");
		private static readonly int Results_ID = Shader.PropertyToID("Results");

		[SerializeField] private ComputeShader computeShader;

		private uint threadSize = 0;

		public static DepthToWorld Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			computeShader.GetKernelThreadGroupSizes(0, out threadSize, out uint y, out uint z);
		}

		public static bool Sample(Vector2[] uvs, out Vector3[] results) => Instance.SamplePositions(uvs, out results);

		public bool SamplePositions(Vector2[] uvs, out Vector3[] results)
		{
			results = new Vector3[uvs.Length];

			if (!DepthKitDriver.DepthAvailable)
			{
				if (Debug.isDebugBuild)
					Debug.Log("Depth incapable or disabled! Falling back to floorcast...");

				return false;
			}

			int threads = Mathf.CeilToInt(uvs.Length / (float)threadSize);

			var requestsCB = new ComputeBuffer(uvs.Length, Marshal.SizeOf<Vector2>(), ComputeBufferType.Structured);
			var resultsCB = new ComputeBuffer(uvs.Length, Marshal.SizeOf<Vector3>(), ComputeBufferType.Structured);

			requestsCB.SetData(uvs);

			computeShader.SetBuffer(0, Requests_ID, requestsCB);
			computeShader.SetBuffer(0, Results_ID, resultsCB);

			computeShader.Dispatch(0, threads, 1, 1);

			results = new Vector3[uvs.Length];
			resultsCB.GetData(results);

			requestsCB.Release();
			resultsCB.Release();

			return true;
		}

		private void Update()
		{
			if (!DepthKitDriver.DepthAvailable)
				return;

			computeShader.SetTexture(0, DepthKitDriver.agDepthTex_ID,
					Shader.GetGlobalTexture(DepthKitDriver.agDepthTex_ID));
		}
	}
}