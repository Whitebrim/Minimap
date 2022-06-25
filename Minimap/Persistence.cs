using System;

namespace Whitebrim.Minimap
{
	[Serializable]
	public class Persistence
	{
		public int[] QualityDefinitions = new[] { 64, 128, 192, 256, 384, 512 };

		public string zoomMinimapOut = null;
		public string zoomMinimapIn = null;
		public string minimapDrag = null;
		public float zoomSpeed = 1;
		public float nearClip = 200;
		public int minimapPosition = 0;
		public bool caveMode = false;
		public bool markers = true;
		public int renderingQuality = 3;
		public float defaultZoom = 15;
	}
}