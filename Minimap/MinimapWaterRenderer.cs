using UltimateWater;
using UltimateWater.Internal;
using UnityEngine;

namespace Whitebrim.Minimap
{
	/// <summary>
	/// Ensures the minimap camera's culling mask includes the water layer
	/// so that WaterCamera.OnPreCull (Effect type) can render water.
	/// Runs in Update (before OnPreCull) to guarantee the mask is set in time.
	/// </summary>
	public class MinimapWaterRenderer : MonoBehaviour
	{
		private Camera cam;
		private bool initialized;

		private void Awake()
		{
			cam = GetComponent<Camera>();
		}

		private void Update()
		{
			if (initialized) return;

			var waterSystem = ApplicationSingleton<WaterSystem>.Instance;
			if (waterSystem == null) return;

			var waters = waterSystem.Waters;
			if (waters.Count == 0) return;

			for (int i = waters.Count - 1; i >= 0; i--)
				cam.cullingMask |= (1 << waters[i].gameObject.layer);

			initialized = true;
		}
	}
}
