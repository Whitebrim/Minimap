using UnityEngine;

namespace Whitebrim.Minimap
{
	public class MarkerMover : MonoBehaviour
	{
		public bool active = true;
		public Vector3 minimapShift { get { return new Vector3(0, Random.Range(-0.2f, 0.2f), 0); } }
		private Quaternion defaultRotation = Quaternion.Euler(90, 0, 0);
		private Transform parent;

		private Vector3 staticMinimapShift;

		private void Awake()
		{
			staticMinimapShift = minimapShift;
		}

		private void LateUpdate()
		{
			if (parent == null)
			{
				parent = transform.parent;
			}
			if (active)
			{
				transform.position = parent.position + staticMinimapShift;
				transform.rotation = defaultRotation * Quaternion.Euler(0, 0, -parent.transform.rotation.eulerAngles.y);
			}
		}
	}
}
