using UnityEngine;
using AprilTag;
using System;
using System.Collections.Generic;

public class QuestAprilTagTracker : MonoBehaviour
{
	private TagDetector detector;

	[SerializeField] private QuestScreenCaptureTextureManager screenCaptureTextureManager;

	// On Quest, MediaProjection view seems to come from left eye
	// so this should probably be the left eye transform
	public Transform physicalCamRepresentation;

	public float horizontalFovDeg = 80f;
	public float tagSizeMeters = 0.12f;
	[SerializeField] private int decimation = 4;

	[NonSerialized] public Texture2D texture;

	private Matrix4x4 headPoseAtPrevFrame;
	private Matrix4x4 headPoseAtLatestFrame;

	private List<TagPose> worldPoses = new(10);
	public event Action<IEnumerable<TagPose>> OnDetectTags = delegate { };

	void Start()
	{
		detector = new TagDetector(1024, 1024, decimation);

		if(physicalCamRepresentation == null)
			physicalCamRepresentation = FindObjectOfType<OVRCameraRig>().leftEyeAnchor;
	}

	private void OnEnable()
	{
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
		headPoseAtPrevFrame = headPoseAtLatestFrame;
		headPoseAtLatestFrame = physicalCamRepresentation.localToWorldMatrix;
	}

	// called on another thread so we do this
	private bool newFrameAvailable = false;
	private void OnReceivedNewFrame()
	{
		newFrameAvailable = true;
	}

	private void Update()
	{
		if (!newFrameAvailable || headPoseAtPrevFrame == default) return;

		detector.ProcessImage(texture.GetPixels32(), horizontalFovDeg * Mathf.Deg2Rad, tagSizeMeters);

		worldPoses.Clear();
		foreach (var pose in detector.DetectedTags)
		{
			TagPose worldPose = new(
				pose.ID,
				headPoseAtPrevFrame.MultiplyPoint(pose.Position),
				headPoseAtPrevFrame.rotation * pose.Rotation * Quaternion.Euler(-90, 0, 0)
				);

			worldPoses.Add(worldPose);
		}

		OnDetectTags.Invoke(worldPoses);

		headPoseAtPrevFrame = default;
		newFrameAvailable = false;
	}
}
