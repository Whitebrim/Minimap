using UnityEngine;

namespace Whitebrim.Minimap
{
	public class MinimapRotator : MonoBehaviour
	{
		public RectTransform Compass;
		public Transform Camera;

		private void LateUpdate()
		{
			if (Camera is object)
			{
				Compass.localRotation = Quaternion.Euler(0, 0, Camera.rotation.eulerAngles.y);
			}
		}
	}
}