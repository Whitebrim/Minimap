﻿using UnityEngine;

namespace Whitebrim.Minimap
{
	public class MinimapCameraMover : MonoBehaviour
	{
		public bool active = true;
		public Vector3 minimapShift = new Vector3(0, 300, 0);
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
				transform.position = new Vector3(parent.position.x, staticMinimapShift.y, parent.position.z);
				transform.rotation = defaultRotation * Quaternion.Euler(0, 0, -parent.transform.rotation.eulerAngles.y);
			}
		}
	}
}