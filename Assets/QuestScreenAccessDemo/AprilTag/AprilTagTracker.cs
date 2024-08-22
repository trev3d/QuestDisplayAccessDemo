using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AprilTag;
using AprilTag.Interop;
using System;

public class AprilTagTracker : MonoBehaviour
{
	private TagDetector detector;

	[SerializeField] private GameObject aprilTagIndicatorPrefab;
	[SerializeField] private int numTagsToSpawn;

	[SerializeField] private float horizontalFovDeg = 110f;
	[SerializeField] private float tagSizeMeters = 0.21f;
	[SerializeField] private int decimation = 2;

	public Transform headTransform;

	private GameObject[] indicators;

	void Start()
	{
		detector = new TagDetector(1024, 1024, decimation);

		indicators = new GameObject[numTagsToSpawn];

		for (int i = 0; i < numTagsToSpawn; i++)
		{
			indicators[i] = Instantiate(aprilTagIndicatorPrefab);
		}

		MediaProjectionTextureGetter.Instance.OnReceivedNewFrame += OnReceivedNewFrame;
	}

	private void OnReceivedNewFrame()
	{
		if (MediaProjectionTextureGetter.DisplayTexture == null) return;

		UnityEngine.Pose t = new UnityEngine.Pose(headTransform.position, headTransform.rotation);

		Matrix4x4 mat = Matrix4x4.TRS(headTransform.position, headTransform.rotation, Vector3.one);

		detector.ProcessImage(MediaProjectionTextureGetter.DisplayTexture.GetPixels32(), horizontalFovDeg * Mathf.Deg2Rad, tagSizeMeters);

		int index = 0;

		foreach (var tag in detector.DetectedTags)
		{
			if (index >= indicators.Length)
				break;

			GameObject indicator = indicators[index];

			Vector3 pos = tag.Position;
			pos /= 1.8f;

			indicator.transform.position = mat.MultiplyPoint(pos);
			indicator.transform.rotation = mat.rotation * tag.Rotation * Quaternion.Euler(-90, 0, 0);

			index++;
		}
	}
}
