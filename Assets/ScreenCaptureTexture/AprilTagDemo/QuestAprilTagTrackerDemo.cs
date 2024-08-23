using UnityEngine;
using AprilTag;

public class QuestAprilTagTrackerDemo : MonoBehaviour
{
	private TagDetector detector;

	[SerializeField] private GameObject aprilTagIndicatorPrefab;
	[SerializeField] private int numTagsToSpawn;

	[SerializeField] private float horizontalFovDeg = 110f;
	[SerializeField] private float tagSizeMeters = 0.21f;
	[SerializeField] private int decimation = 2;

	// On Quest, MediaProjection view seems to come from left eye
	// so this should probably be the left eye transform
	public Transform headTransform;

	public Texture2D texture;

	private GameObject[] indicators;

	private Matrix4x4 headPoseAtPrevFrame;
	private Matrix4x4 headPoseAtLatestFrame;

	void Start()
	{
		detector = new TagDetector(1024, 1024, decimation);

		indicators = new GameObject[numTagsToSpawn];

		for (int i = 0; i < numTagsToSpawn; i++)
		{
			indicators[i] = Instantiate(aprilTagIndicatorPrefab);
		}

		if(headTransform == null)
			headTransform = FindObjectOfType<OVRCameraRig>().leftEyeAnchor;

		texture = ScreenCaptureTextureManager.ScreenCaptureTexture;

		ScreenCaptureTextureManager.Instance.OnNewFrameIncoming.AddListener(CacheHeadPose);
		ScreenCaptureTextureManager.Instance.OnNewFrame.AddListener(OnReceivedNewFrame);
	}

	private void SetTexture(Texture2D texture) => this.texture = texture;

	private void CacheHeadPose()
	{
		headPoseAtPrevFrame = headPoseAtLatestFrame;
		headPoseAtLatestFrame = headTransform.localToWorldMatrix;
	}

	// called on another thread so we do this
	private bool newFrameAvailable = false;
	private void OnReceivedNewFrame()
	{
		newFrameAvailable = true;
	}

	private void Update()
	{
		if (!newFrameAvailable) return;

		Debug.Log("Tracking tags");

		detector.ProcessImage(texture.GetPixels32(), horizontalFovDeg * Mathf.Deg2Rad, tagSizeMeters);

		int index = 0;

		foreach (var tag in detector.DetectedTags)
		{
			if (index >= indicators.Length)
				break;

			GameObject indicator = indicators[index];

			Vector3 pos = tag.Position;

			indicator.transform.position = headPoseAtPrevFrame.MultiplyPoint(pos);
			indicator.transform.rotation = headPoseAtPrevFrame.rotation * tag.Rotation * Quaternion.Euler(-90, 0, 0);

			index++;
		}

		newFrameAvailable = false;
	}
}
