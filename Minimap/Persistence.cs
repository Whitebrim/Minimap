using System;

namespace Whitebrim.Minimap
{
	[Serializable]
	class Persistence
	{
		public string zoomMinimapOut = null;
		public string zoomMinimapIn = null;
		public string minimapDrag = null;
		public float zoomSpeed = 1;
		public float nearClip = 200;
		public int minimapPosition = 0;
		public bool caveMode = false;
	}
}
