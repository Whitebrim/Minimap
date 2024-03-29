﻿using HarmonyLib;
using UnityEngine;

namespace Whitebrim.Minimap
{
	public class Patches
	{
		[HarmonyPatch(typeof(AI_NetworkBehaviour_Animal), "Awake")]
		private class Awake
		{
			private static void Postfix(AI_NetworkBehaviour_Animal __instance)
			{
				if (Minimap.Instance.persistence.markers)
				{
					if (__instance is AI_NetworkBehavior_Shark ||
						__instance is AI_NetworkBehaviour_Dolphin ||
						__instance is AI_NetworkBehaviour_Whale)
					{
						Minimap.AddMarker(__instance.transform, Minimap.MarkerType.SHARK);
					}
					else
					{
						if (__instance is AI_NetworkBehaviour_Bear ||
							__instance is AI_NetworkBehaviour_Boar ||
							__instance is AI_NetworkBehaviour_ButlerBot ||
							__instance is AI_NetworkBehaviour_MamaBear ||
							__instance is AI_NetworkBehaviour_Pig ||
							__instance is AI_NetworkBehaviour_PufferFish ||
							__instance is AI_NetworkBehaviour_Rat ||
							__instance is AI_NetworkBehaviour_StoneBird ||
							__instance is AI_NetworkBehaviour_Boss_Varuna ||
							__instance is AI_NetworkBehaviour_PolarBear ||
							__instance is AI_NetworkBehaviour_Hyena ||
							__instance is AI_NetworkBehaviour_HyenaBoss ||
							__instance is AI_NetworkBehaviour_Roach)
						{
							Minimap.AddMarker(__instance.transform, Minimap.MarkerType.ENEMY);
						}
						else if (__instance is AI_NetworkBehaviour_NPC)
						{
							Minimap.AddMarker(__instance.transform, Minimap.MarkerType.NPC);
						}
						else if (__instance is AI_NetworkBehaviour_BugSwarm ||
							__instance is AI_NetworkBehaviour_Chicken ||
							__instance is AI_NetworkBehaviour_Goat ||
							__instance is AI_NetworkBehaviour_Llama ||
							__instance is AI_NetworkBehaviour_Animal)
						{
							Minimap.AddMarker(__instance.transform, Minimap.MarkerType.NEUTRAL);
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Network_Player), "Start")]
		private class Start
		{
			private static void Prefix(Network_Player __instance)
			{
				if (Minimap.Instance.persistence.markers)
				{
					if (!Equals(__instance, RAPI.GetLocalPlayer()))
					{
						Minimap.AddMarker(__instance.transform, Minimap.MarkerType.PLAYER);
					}
				}
			}
		}
	}
}