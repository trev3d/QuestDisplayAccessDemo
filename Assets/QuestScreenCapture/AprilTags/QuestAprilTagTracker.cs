using UnityEngine;
using AprilTag;
using System;
using System.Collections.Generic;
using UnityEngine.XR;
using Trev3d.Quest.ScreenCapture;

namespace Trev3d.Quest.AprilTags
{
	[DefaultExecutionOrder(-1000)]
	public class QuestAprilTagTracker : MonoBehaviour
	{
		public static QuestAprilTagTracker Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		private TagDetector detector;

		[SerializeField] private QuestScreenCaptureTextureManager screenCaptureTextureManager;

		public float horizontalFovDeg = 80f;
		public float tagSizeMeters = 0.12f;
		[SerializeField] private int decimation = 4;

		[NonSerialized] public Texture2D texture;

		private Matrix4x4 headPoseWhenLastFrameIncoming;
		private Matrix4x4 headPoseWhenNewFrameIncoming;

		public bool detectionEnabled = true;
		public void SetDetectionEnabled(bool b) => detectionEnabled = b;

		private List<TagPose> worldPoses = new(10);
		public event Action<IEnumerable<TagPose>> OnDetectTags = delegate { };

		private List<XRNodeState> nodeStates = new();

		void Start()
		{
			if (screenCaptureTextureManager.ScreenCaptureTexture != null)
				texture = screenCaptureTextureManager.ScreenCaptureTexture;

			detector = new TagDetector(QuestScreenCaptureTextureManager.Size.x, QuestScreenCaptureTextureManager.Size.y, decimation);
		}

		private void OnEnable()
		{
			if (screenCaptureTextureManager.ScreenCaptureTexture != null)
				texture = screenCaptureTextureManager.ScreenCaptureTexture;

			screenCaptureTextureManager.OnNewFrameIncoming.AddListener(CacheHeadPose);
			screenCaptureTextureManager.OnNewFrame.AddListener(OnReceivedNewFrame);
		}

		private void OnDisable()
		{
			screenCaptureTextureManager.OnNewFrameIncoming.RemoveListener(CacheHeadPose);
			screenCaptureTextureManager.OnNewFrame.RemoveListener(OnReceivedNewFrame);
		}

		private void SetTexture(Texture2D texture) => this.texture = texture;

		private void CacheHeadPose()
		{
			headPoseWhenLastFrameIncoming = headPoseWhenNewFrameIncoming;

			InputTracking.GetNodeStates(nodeStates);

			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;

			for (int i = 0; i < nodeStates.Count; i++)
			{
				XRNodeState state = nodeStates[i];
				if (state.nodeType == XRNode.LeftEye)
				{
					state.TryGetPosition(out pos);
					state.TryGetRotation(out rot);
					break;
				}
			}

			headPoseWhenNewFrameIncoming = Matrix4x4.TRS(pos, rot, Vector3.one);
		}

		// called on another thread so we do this
		private bool newFrameAvailable = false;
		private void OnReceivedNewFrame()
		{
			newFrameAvailable = true;
		}

		private void Update()
		{
			if (!detectionEnabled || !newFrameAvailable || headPoseWhenLastFrameIncoming == default) return;

			detector.ProcessImage(texture.GetPixels32(), horizontalFovDeg * Mathf.Deg2Rad, tagSizeMeters);

			worldPoses.Clear();
			foreach (var pose in detector.DetectedTags)
			{
				TagPose worldPose = new(
					pose.ID,
					headPoseWhenLastFrameIncoming.MultiplyPoint(pose.Position),
					headPoseWhenLastFrameIncoming.rotation * pose.Rotation * Quaternion.Euler(-90, 0, 0)
					);

				worldPoses.Add(worldPose);
			}

			newFrameAvailable = false;
			headPoseWhenLastFrameIncoming = default;

			OnDetectTags.Invoke(worldPoses);
		}
	}
}