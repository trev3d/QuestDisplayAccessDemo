using UnityEngine;

namespace Trev3d.Quest.AprilTags
{
	public class TagFollower : MonoBehaviour
	{
		[SerializeField] private int followTagID = 0;

		public void FollowTag(int id)
		{
			if (enabled)
			{
				TagFollowerMover.Instance.allFollowers.Remove(followTagID);
				TagFollowerMover.Instance.allFollowers.Add(id, this);
			}

			followTagID = id;
		}

		private void OnEnable() => TagFollowerMover.Instance.allFollowers.Add(followTagID, this);

		private void OnDisable() => TagFollowerMover.Instance.allFollowers.Remove(followTagID);
	}
}