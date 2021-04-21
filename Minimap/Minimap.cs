using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UltimateWater;
using UnityEngine;


namespace Whitebrim.Minimap
{
	public class Minimap : Mod
	{
		public enum MarkerType
		{
			ENEMY,
			NEUTRAL,
			PLAYER,
			SHARK
		}

		public const int MASK_LAYER = 1 << 18;
		public const float MARKS_SCALE = 20;
		public const float CAVE_MODE_CLIP_TOP = 1f;
		public const float CAVE_MODE_CLIP_BOTTOM = 2f;
		public static readonly Color PLAYER_COLOR = new Color(0x47 / 255f, 0xAB / 255f, 0x3C / 255f);
		public static readonly Color ENEMY_COLOR = new Color(0xCC / 255f, 0x31 / 255f, 0x48 / 255f);
		public static readonly Color NEUTRAL_COLOR = new Color(0xE0 / 255f, 0xA9 / 255f, 0x18 / 255f);
		public static readonly Color SHARK_COLOR = new Color(0x26 / 255f, 0x79 / 255f, 0xCC / 255f);

		public static Minimap Instance => (Minimap)modInstance;

		// Extra Settings API
		public static Traverse ExtraSettingsAPI_Traverse;
		public static bool ExtraSettingsAPI_Loaded = false;
		public Persistence persistence = new Persistence();

		// Harmony
		private Harmony harmonyInstance;

		private AssetBundle asset;
		private Camera camera;
		private GameObject canvas;
		private GameObject marker;
		private TextMeshProUGUI zoomText;
		private List<GameObject> markers = new List<GameObject>();

		private bool allowedToDrag = false;
		private bool markersLastValue;
		private bool loaded = false;

		#region Load

		public void Awake()
		{
			harmonyInstance = new Harmony("com.whitebrim.minimap");
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			PatchAllCameras();
			StartCoroutine(LoadAssets());
		}

		private IEnumerator LoadAssets()
		{
			AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromMemoryAsync(GetEmbeddedFileBytes("minimap.assets"));
			yield return bundleRequest;
			asset = bundleRequest.assetBundle;
			marker = asset.LoadAsset<GameObject>("Minimap Mark");
			if (RAPI.IsCurrentSceneGame())
			{
				InstantiateAssets();
			}
			loaded = true;
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
			Debug.Log("Mod Minimap has been loaded!");
		}

		#endregion

		#region Events

		public void ExtraSettingsAPI_Load()
		{
			persistence.zoomMinimapIn = ExtraSettings.GetKeybindName("zoomminimapin");
			persistence.zoomMinimapOut = ExtraSettings.GetKeybindName("zoomminimapout");
			persistence.minimapDrag = ExtraSettings.GetKeybindName("minimapdrag");
			SavePersistence();
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			markersLastValue = persistence.markers;
			if (RAPI.IsCurrentSceneGame())
			{
				StartCoroutine(WaitForEndOfInitThenAddForgottenMarkers());
			}
		}

		public void ExtraSettingsAPI_SettingsOpen()
		{
			ExtraSettings.SetSliderValue("zoomspeed", persistence.zoomSpeed);
			ExtraSettings.SetSliderValue("nearclip", persistence.nearClip);
			ExtraSettings.SetComboboxSelectedIndex("position", persistence.minimapPosition);
			ExtraSettings.SetCheckboxState("markers", persistence.markers);
			ExtraSettings.SetCheckboxState("cavemode", persistence.caveMode);
		}

		public void ExtraSettingsAPI_SettingsClose()
		{
			SavePersistence();
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateCaveMode();
			UpdateMarkers();
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
					camera.nearClipPlane = 300 - (playerY + CAVE_MODE_CLIP_TOP);
					camera.farClipPlane = 300 + (-playerY + CAVE_MODE_CLIP_BOTTOM);
				}
			}
		}

		#endregion

		#region Unload
		public void OnModUnload()
		{
			harmonyInstance.UnpatchAll();
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
			markers.ForEach((m) => Destroy(m));
			PatchAllCameras(false);
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
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy as T;
		}

		private void SavePersistence()
		{
			persistence.zoomSpeed = ExtraSettings.GetSliderValue("zoomspeed");
			persistence.nearClip = ExtraSettings.GetSliderValue("nearclip");
			persistence.minimapPosition = ExtraSettings.GetComboboxSelectedIndex("position");
			persistence.markers = ExtraSettings.GetCheckboxState("markers");
			persistence.caveMode = ExtraSettings.GetCheckboxState("cavemode");
		}

		private static void ChangeZoom(float newZoom)
		{
			Instance.camera.orthographicSize = newZoom;
			if (!Instance.persistence.caveMode)
			{
				Instance.camera.farClipPlane = Mathf.Max(500, 300 + newZoom);
			}
			Instance.zoomText.text = newZoom.ToString("0.#") + "x";
			for (int i = Instance.markers.Count - 1; i >= 0; i--)
			{
				if (Instance.markers[i] == null)
				{
					Instance.markers.RemoveAt(i);
					continue;
				}
				float scale = (newZoom / MARKS_SCALE) / Instance.markers[i].transform.parent.localScale.x;
				Instance.markers[i].transform.localScale = new Vector3(scale, scale, 0);
			}
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

		private void UpdateMarkers()
		{
			if (RAPI.IsCurrentSceneGame() && persistence.markers != markersLastValue)
			{
				if (persistence.markers)
				{
					AddForgottenMarkers();
				}
				else
				{
					markers.ForEach((m) => Destroy(m));
				}
			}
			markersLastValue = persistence.markers;
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

		private void PatchAllCameras(bool remove = true)
		{
			var cameras = FindObjectsOfType<Camera>();
			foreach (var c in cameras)
			{
				if (c.name != "Minimap Camera")
				{
					if (remove)
					{
						c.cullingMask &= ~MASK_LAYER;
					}
					else
					{
						c.cullingMask |= MASK_LAYER;
					}
				}
			}
		}

		public static void AddMarker(Transform target, MarkerType markerType)
		{
			float scale = ((Instance.camera != null ? Instance.camera.orthographicSize : 15) / MARKS_SCALE) / target.localScale.x;
			var newMarker = Instantiate(Instance.marker, target);
			newMarker.AddComponent<MarkerMover>();
			Instance.markers.Add(newMarker);
			switch (markerType)
			{
				default:
				case MarkerType.ENEMY:
					newMarker.GetComponent<SpriteRenderer>().color = ENEMY_COLOR;
					break;
				case MarkerType.NEUTRAL:
					newMarker.GetComponent<SpriteRenderer>().color = NEUTRAL_COLOR;
					break;
				case MarkerType.PLAYER:
					newMarker.GetComponent<SpriteRenderer>().color = PLAYER_COLOR;
					break;
				case MarkerType.SHARK:
					newMarker.GetComponent<SpriteRenderer>().color = SHARK_COLOR;
					break;
			}
			newMarker.transform.localScale = new Vector3(scale, scale, 0);
		}

		private void AddForgottenMarkers()
		{
			if (RAPI.IsCurrentSceneGame() && persistence.markers)
			{
				var animals = FindObjectsOfType<AI_NetworkBehaviour_Animal>();
				foreach (var entity in animals)
				{
					if (entity is AI_NetworkBehavior_Shark)
					{
						AddMarker(entity.transform, MarkerType.SHARK);
					}
					if (entity is AI_NetworkBehaviour_Bear ||
						entity is AI_NetworkBehaviour_Boar ||
						entity is AI_NetworkBehaviour_ButlerBot ||
						entity is AI_NetworkBehaviour_MamaBear ||
						entity is AI_NetworkBehaviour_Pig ||
						entity is AI_NetworkBehaviour_PufferFish ||
						entity is AI_NetworkBehaviour_Rat ||
						entity is AI_NetworkBehaviour_StoneBird)
					{
						AddMarker(entity.transform, MarkerType.ENEMY);
					}
					if (entity is AI_NetworkBehaviour_BugSwarm ||
						entity is AI_NetworkBehaviour_Chicken ||
						entity is AI_NetworkBehaviour_Goat ||
						entity is AI_NetworkBehaviour_Llama)
					{
						AddMarker(entity.transform, MarkerType.NEUTRAL);
					}
				}
				var players = FindObjectsOfType<Network_Player>();
				var me = RAPI.GetLocalPlayer();
				foreach (var player in players)
				{
					if (player != me)
					{
						AddMarker(player.transform, MarkerType.PLAYER);
					}
				}
			}
		}

		private IEnumerator WaitForEndOfInitThenAddForgottenMarkers()
		{
			while (!loaded)
			{
				yield return new WaitForEndOfFrame();
			}
			UpdateCaveMode();
			AddForgottenMarkers();
		}

		#endregion
	}
}