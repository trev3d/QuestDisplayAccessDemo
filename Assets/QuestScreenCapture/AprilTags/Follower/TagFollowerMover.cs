using AprilTag;
using System.Collections.Generic;
using UnityEngine;

namespace Trev3d.Quest.AprilTags
{
	public class TagFollowerMover : MonoBehaviour
	{
		public static TagFollowerMover Instance { get; private set; }

		public Dictionary<int, TagFollower> allFollowers = new();

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable() => QuestAprilTagTracker.Instance.OnDetectTags += OnDetectTags;
		private void OnDisable() => QuestAprilTagTracker.Instance.OnDetectTags -= OnDetectTags;

		private void OnDetectTags(IEnumerable<TagPose> tagPoses)
		{
			foreach (TagPose tagPose in tagPoses)
			{
				if (!allFollowers.ContainsKey(tagPose.ID))
					continue;

				TagFollower follower = allFollowers[tagPose.ID];

				follower.transform.SetPositionAndRotation(tagPose.Position, tagPose.Rotation);
			}
		}
	}
}