﻿using HarmonyLib;
using System;
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
			SHARK,
			NPC
		}

		public const int MASK_LAYER = 1 << 18;
		public const float MARKS_SCALE = 20;
		public const float CAVE_MODE_CLIP_TOP = 1f;
		public const float CAVE_MODE_CLIP_BOTTOM = 2f;
		public static readonly Color PLAYER_COLOR = new Color(0x47 / 255f, 0xAB / 255f, 0x3C / 255f); // #47AB3C
		public static readonly Color ENEMY_COLOR = new Color(0xCC / 255f, 0x31 / 255f, 0x48 / 255f); // #CC3148
		public static readonly Color NEUTRAL_COLOR = new Color(0xE0 / 255f, 0xA9 / 255f, 0x18 / 255f); // #E0A918
		public static readonly Color SHARK_COLOR = new Color(0x26 / 255f, 0x79 / 255f, 0xCC / 255f); // #2679CC
		public static readonly Color NPC_COLOR = new Color(0xD7 / 255f, 0xB1 / 255f, 0xAB / 255f); // #D7B1AB

		public static Minimap Instance;

		// Extra Settings API
		public static Traverse ExtraSettingsAPI_Traverse;

		public static bool ExtraSettingsAPI_Loaded = false;
		public Persistence persistence = new Persistence();

		// Harmony
		private Harmony harmonyInstance;

		private Canvas[] canvases;
		private Traverse<Canvas[]> traverse;
		private int originalSize = -1;

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
			Instance = this;
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
				yield return StartCoroutine(InstantiateAssets());
			}
			else
			{
				Debug.Log("[<color=#DBBF63>Minimap</color>] will be loaded in game.");
			}
			loaded = true;
		}

		private IEnumerator InstantiateAssets()
		{
			while (RAPI.GetLocalPlayer() == null)
			{
				yield return new WaitForEndOfFrame();
			}
			var cameraPrefab = asset.LoadAsset<GameObject>("Minimap Camera");
			camera = Instantiate(cameraPrefab, RAPI.GetLocalPlayer().transform).GetComponent<Camera>();
			cameraPrefab.GetComponent<Camera>().targetTexture = null;
			var waterCamera = CopyComponent(Camera.main.GetComponent<WaterCamera>(), camera.gameObject);
			waterCamera.ReflectionCamera = null;
			CopyComponent(Camera.main.GetComponent<WaterCameraIME>(), camera.gameObject);
			camera.gameObject.AddComponent<MinimapCameraMover>();
			var canvasPrefab = asset.LoadAsset<GameObject>("_MinimapCanvas");
			canvas = Instantiate(canvasPrefab, GameObject.Find("Canvases").transform);
			traverse = Traverse.Create(GameObject.Find("_CanvasGame_New").GetComponent<CanvasHelper>()).Field<Canvas[]>("canvases");
			canvases = traverse.Value;
			if (originalSize == -1)
			{
				originalSize = canvases.Length;
			}
			Array.Resize(ref canvases, originalSize + 1);
			canvases[canvases.Length - 1] = canvas.GetComponent<Canvas>();
			traverse.Value = canvases;
			zoomText = canvas.transform.FindChildRecursively("ZoomText").GetComponent<TextMeshProUGUI>();
			var script = canvas.AddComponent<MinimapRotator>();
			script.Camera = camera.transform;
			script.Compass = canvas.transform.FindChildRecursively("Compass") as RectTransform;
			Debug.Log("[<color=#DBBF63>Minimap</color>] has been loaded!");
		}

		#endregion Load

		#region Events

		public void ExtraSettingsAPI_Load()
		{
			persistence.zoomMinimapIn = ExtraSettings.GetKeybindName("zoomminimapin");
			persistence.zoomMinimapOut = ExtraSettings.GetKeybindName("zoomminimapout");
			persistence.minimapDrag = ExtraSettings.GetKeybindName("minimapdrag");
			SavePersistence();

			markersLastValue = persistence.markers;
			if (RAPI.IsCurrentSceneGame())
			{
				StartCoroutine(WaitForEndOfInitInGameScene());
			}
		}

		public void ExtraSettingsAPI_SettingsOpen()
		{
			ExtraSettings.SetSliderValue("zoomspeed", persistence.zoomSpeed);
			ExtraSettings.SetSliderValue("nearclip", persistence.nearClip);
			ExtraSettings.SetComboboxSelectedIndex("position", persistence.minimapPosition);
			ExtraSettings.SetComboboxSelectedIndex("renderquality", persistence.renderingQuality);
			ExtraSettings.SetCheckboxState("markers", persistence.markers);
			ExtraSettings.SetCheckboxState("cavemode", persistence.caveMode);
			ExtraSettings.SetInputValue("defaultzoom", persistence.defaultZoom.ToString());
		}

		public void ExtraSettingsAPI_SettingsClose()
		{
			SavePersistence();

			if (RAPI.IsCurrentSceneGame())
			{
				UpdateRenderSettings();
				UpdateMinimapPosition();
				UpdateCameraNearClip();
				UpdateCaveMode();
				UpdateMarkers();
				ChangeZoom(persistence.defaultZoom);
			}
		}

		public override void WorldEvent_WorldLoaded()
		{
			StartCoroutine(WorldLoadedCoroutine());
		}

		public void ExtraSettingsAPI_ButtonPress(string name)
		{
			if (name == "default")
			{
				ExtraSettings.ResetAllSettings();
			}
		}

		#endregion Events

		#region Update

		public void Update()
		{
			if (ExtraSettingsAPI_Loaded && camera != null)
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

		#endregion Update

		#region Unload

		public void OnModUnload()
		{
			harmonyInstance.UnpatchAll("com.whitebrim.minimap");
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
			if (originalSize != -1)
			{
				Array.Resize(ref canvases, originalSize);
				traverse.Value = canvases;
			}
			PatchAllCameras(false);
			Debug.Log("[<color=#DBBF63>Minimap</color>] has been unloaded!");
		}

		#endregion Unload

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

		#endregion Commands

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
			persistence.renderingQuality = ExtraSettings.GetComboboxSelectedIndex("renderquality");
			persistence.defaultZoom = Mathf.Max(1, float.Parse(ExtraSettings.GetInputValue("defaultzoom") is null ? "15" : ExtraSettings.GetInputValue("defaultzoom")));
		}

		private static void ChangeZoom(float newZoom)
		{
			if (Instance.camera is null) return;

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

		private void UpdateRenderSettings()
		{
			if (camera != null && ExtraSettingsAPI_Loaded)
			{
				var targetTexture = camera.targetTexture;
				if (targetTexture.width != persistence.QualityDefinitions[persistence.renderingQuality])
				{
					targetTexture.Release();
					targetTexture.width = targetTexture.height = persistence.QualityDefinitions[persistence.renderingQuality];
					targetTexture.Create();
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
				case MarkerType.NPC:
					newMarker.GetComponent<SpriteRenderer>().color = NPC_COLOR;
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
					if (entity is AI_NetworkBehavior_Shark ||
						entity is AI_NetworkBehaviour_Dolphin ||
						entity is AI_NetworkBehaviour_Whale)
					{
						AddMarker(entity.transform, MarkerType.SHARK);
					}
					else if (entity is AI_NetworkBehaviour_Bear ||
						entity is AI_NetworkBehaviour_Boar ||
						entity is AI_NetworkBehaviour_ButlerBot ||
						entity is AI_NetworkBehaviour_MamaBear ||
						entity is AI_NetworkBehaviour_Pig ||
						entity is AI_NetworkBehaviour_PufferFish ||
						entity is AI_NetworkBehaviour_Rat ||
						entity is AI_NetworkBehaviour_StoneBird ||
						entity is AI_NetworkBehaviour_Boss_Varuna ||
						entity is AI_NetworkBehaviour_PolarBear ||
						entity is AI_NetworkBehaviour_Hyena ||
						entity is AI_NetworkBehaviour_HyenaBoss)
					{
						AddMarker(entity.transform, MarkerType.ENEMY);
					}
					else if (entity is AI_NetworkBehaviour_NPC)
					{
						AddMarker(entity.transform, MarkerType.NPC);
					}
					else if (entity is AI_NetworkBehaviour_BugSwarm ||
						entity is AI_NetworkBehaviour_Chicken ||
						entity is AI_NetworkBehaviour_Goat ||
						entity is AI_NetworkBehaviour_Llama ||
						entity is AI_NetworkBehaviour_Animal)
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

		private IEnumerator WaitForEndOfInitInGameScene()
		{
			while (!loaded)
			{
				yield return new WaitForEndOfFrame();
			}
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateRenderSettings();
			UpdateCaveMode();
			AddForgottenMarkers();
			ChangeZoom(persistence.defaultZoom);
		}

		private IEnumerator WorldLoadedCoroutine()
		{
			PatchAllCameras();
			yield return StartCoroutine(InstantiateAssets());
			UpdateRenderSettings();
			UpdateMinimapPosition();
			UpdateCameraNearClip();
			UpdateCaveMode();
			ChangeZoom(persistence.defaultZoom);
		}

		#endregion Misc
	}
}