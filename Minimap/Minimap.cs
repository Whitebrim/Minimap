using System.Collections;
using TMPro;
using UltimateWater;
using UnityEngine;


namespace Whitebrim.Minimap
{
	public class Minimap : Mod
	{
		public static Minimap Instance => (Minimap)modInstance;

		// Extra Settings API
		public static HarmonyLib.Traverse ExtraSettingsAPI_Traverse;
		public static bool ExtraSettingsAPI_Loaded = false;
		private Persistence persistence = new Persistence();

		private AssetBundle asset;
		private Camera camera;
		private GameObject canvas;
		private TextMeshProUGUI zoomText;

		private bool allowedToDrag = false;

		#region Load

		public void Awake()
		{
			PatchAllCameras();
			StartCoroutine(LoadAssets());
		}

		private IEnumerator LoadAssets()
		{
			AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes("minimap.assets"));
			yield return bundleRequest;
			asset = bundleRequest.assetBundle;

			Debug.Log("Mod Minimap has been loaded!");
			if (RAPI.IsCurrentSceneGame())
			{
				InstantiateAssets();
			}
		}

		private void InstantiateAssets()
		{
			var cameraPrefab = asset.LoadAsset<GameObject>("Minimap Camera");
			camera = Instantiate(cameraPrefab, RAPI.GetLocalPlayer().transform).GetComponent<Camera>();
			CopyComponent(Camera.main.GetComponent<WaterCamera>(), camera.gameObject);
			CopyComponent(Camera.main.GetComponent<WaterCameraIME>(), camera.gameObject);
			camera.gameObject.AddComponent<MinimapCameraMover>();
			var canvasPrefab = asset.LoadAsset<GameObject>("_MinimapCanvas");
			canvas = Instantiate(canvasPrefab, GameObject.Find("Canvases").transform);
			zoomText = canvas.transform.FindChildRecursively("ZoomText").GetComponent<TextMeshProUGUI>();
			var script = canvas.AddComponent<MinimapRotator>();
			script.Camera = camera.transform;
			script.Compass = canvas.transform.FindChildRecursively("Compass") as RectTransform;
		}

		#endregion

		#region Events

		public void ExtraSettingsAPI_Load()
		{
			persistence.zoomMinimapIn = ExtraSettings.GetKeybindName("zoomminimapin");
			persistence.zoomMinimapOut = ExtraSettings.GetKeybindName("zoomminimapout");
			persistence.minimapDrag = ExtraSettings.GetKeybindName("minimapdrag");
			persistence.zoomSpeed = ExtraSettings.GetSliderValue("zoomspeed");
			persistence.nearClip = ExtraSettings.GetSliderValue("nearclip");
			persistence.minimapPosition = ExtraSettings.GetComboboxSelectedIndex("position");
			persistence.caveMode = ExtraSettings.GetCheckboxState("cavemode");
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateCaveMode();
		}

		public void ExtraSettingsAPI_SettingsOpen()
		{
			ExtraSettings.SetSliderValue("zoomspeed", persistence.zoomSpeed);
			ExtraSettings.SetSliderValue("nearclip", persistence.nearClip);
			ExtraSettings.SetComboboxSelectedIndex("position", persistence.minimapPosition);
			ExtraSettings.SetCheckboxState("cavemode", persistence.caveMode);
		}

		public void ExtraSettingsAPI_SettingsClose()
		{
			persistence.zoomSpeed = ExtraSettings.GetSliderValue("zoomspeed");
			persistence.nearClip = ExtraSettings.GetSliderValue("nearclip");
			persistence.minimapPosition = ExtraSettings.GetComboboxSelectedIndex("position");
			persistence.caveMode = ExtraSettings.GetCheckboxState("cavemode");
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateCaveMode();
		}

		public override void WorldEvent_WorldLoaded()
		{
			PatchAllCameras();
			InstantiateAssets();
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateCaveMode();
		}

		public void ExtraSettingsAPI_ButtonPress(string name)
		{
			if (name == "default")
			{
				ExtraSettings.ResetAllSettings();
			}
		}

		#endregion

		#region Update

		public void Update()
		{
			if (ExtraSettingsAPI_Loaded)
			{
				if (persistence.zoomMinimapIn != null && MyInput.GetButton(persistence.zoomMinimapIn))
				{
					ZoomMinimapIn();
				}
				if (persistence.zoomMinimapOut != null && MyInput.GetButton(persistence.zoomMinimapOut))
				{
					ZoomMinimapOut();
				}
				if (persistence.minimapDrag != null && MyInput.GetButtonDown(persistence.minimapDrag))
				{
					InitMinimapDrag(true);
				}
				if (persistence.minimapDrag != null && MyInput.GetButton(persistence.minimapDrag))
				{
					OnMinimapDrag();
				}
				if (persistence.minimapDrag != null && MyInput.GetButtonUp(persistence.minimapDrag))
				{
					InitMinimapDrag(false);
				}
				if (persistence.caveMode && camera != null)
				{
					var playerY = camera.transform.parent.position.y;
					camera.nearClipPlane = 300 - (playerY + 3);
					camera.farClipPlane = 300 + (-playerY + 3);
				}
			}
		}

		#endregion

		#region Unload
		public void OnModUnload()
		{
			if (camera != null)
			{
				camera.targetTexture = null;
				Destroy(camera.gameObject);
			}
			if (canvas != null)
			{
				Destroy(canvas);
			}
			asset.Unload(true);
			Debug.Log("Mod Minimap has been unloaded!");
		}

		#endregion

		#region Commands

		[ConsoleCommand(name: "zoomminimap", docs: "Change minimap zoom")]
		public static void ZoomMinimap(string[] args)
		{
			if (RAPI.IsCurrentSceneGame())
			{
				ChangeZoom(float.Parse(args[0]));
			}
		}

		[ConsoleCommand(name: "zoomminimapin", docs: "Zoom minimap in")]
		public static void ZoomMinimapIn()
		{
			if (RAPI.IsCurrentSceneGame())
			{
				ChangeZoom(Mathf.Max(1, Instance.camera.orthographicSize - ZoomFunction(Instance.camera.orthographicSize)));
			}
		}

		[ConsoleCommand(name: "zoomminimapout", docs: "Zoom minimap out")]
		public static void ZoomMinimapOut()
		{
			if (RAPI.IsCurrentSceneGame())
			{
				ChangeZoom(Instance.camera.orthographicSize + ZoomFunction(Instance.camera.orthographicSize));
			}
		}

		#endregion

		#region Misc

		private T CopyComponent<T>(T original, GameObject destination) where T : Component
		{
			System.Type type = original.GetType();
			Component copy = destination.AddComponent(type);
			System.Reflection.FieldInfo[] fields = type.GetFields();
			foreach (System.Reflection.FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy as T;
		}

		private static void ChangeZoom(float newZoom)
		{
			Instance.camera.orthographicSize = newZoom;
			if (!Instance.persistence.caveMode)
			{
				Instance.camera.farClipPlane = Mathf.Max(500, 300 + newZoom);
			}
			Instance.zoomText.text = newZoom.ToString("0.#") + "x";
		}

		private static float ZoomFunction(float zoom)
		{
			return Mathf.Sqrt(Mathf.Max(1, zoom)) / 10 * Instance.persistence.zoomSpeed;
		}

		private void UpdateCameraNearClip()
		{
			if (!persistence.caveMode && camera != null && ExtraSettingsAPI_Loaded)
			{
				camera.nearClipPlane = 300 - persistence.nearClip;
			}
		}

		private void UpdateMinimapPosition()
		{
			if (canvas != null && ExtraSettingsAPI_Loaded)
			{
				var rectTransform = canvas.transform.GetChild(0) as RectTransform;
				switch (persistence.minimapPosition)
				{
					default:
					case 0:
						rectTransform.anchorMin = new Vector2(0, 1);
						rectTransform.anchorMax = new Vector2(0, 1);
						rectTransform.pivot = new Vector2(0, 1);
						rectTransform.anchoredPosition = new Vector2(40, -40);
						break;
					case 1:
						rectTransform.anchorMin = new Vector2(1, 1);
						rectTransform.anchorMax = new Vector2(1, 1);
						rectTransform.pivot = new Vector2(1, 1);
						rectTransform.anchoredPosition = new Vector2(-40, -40);
						break;
				}
			}
		}

		private void UpdateCaveMode()
		{
			if (camera != null && ExtraSettingsAPI_Loaded)
			{
				camera.GetComponent<WaterCamera>().enabled = !persistence.caveMode;
				if (!persistence.caveMode)
				{
					camera.nearClipPlane = 100;
					camera.farClipPlane = 500;
					UpdateCameraNearClip();
				}
			}
		}

		private void InitMinimapDrag(bool drag)
		{
			if (RAPI.IsCurrentSceneGame())
			{
				RAPI.GetLocalPlayer().PlayerScript.SetMouseLookScripts(!drag);
				camera.GetComponent<MinimapCameraMover>().active = !drag;
				canvas.transform.FindChildRecursively("Player").gameObject.SetActive(!drag);
				allowedToDrag = drag;
			}
		}

		private void OnMinimapDrag()
		{
			if (allowedToDrag)
			{
				Vector3 mouseDelta = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
				camera.transform.position += (Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0)) * mouseDelta * (camera.orthographicSize / 10f);
			}
		}

		private void PatchAllCameras()
		{
			var cameras = FindObjectsOfType<Camera>();
			foreach (var c in cameras)
			{
				if (c.name != "Minimap Camera")
				{
					c.cullingMask &= ~8;
				}
			}
		}

		#endregion
	}
}