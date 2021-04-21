using HarmonyLib;

namespace Whitebrim.Minimap
{
	class Patches
	{
		[HarmonyPatch(typeof(AI_NetworkBehaviour_Animal), "Awake")]
		class Awake
		{
			private static void Postfix(AI_NetworkBehaviour_Animal __instance)
			{
				if (Minimap.Instance.persistence.markers)
				{
					if (__instance is AI_NetworkBehavior_Shark)
					{
						Minimap.AddMarker(__instance.transform, Minimap.MarkerType.SHARK);
					}
					if (__instance is AI_NetworkBehaviour_Bear ||
						__instance is AI_NetworkBehaviour_Boar ||
						__instance is AI_NetworkBehaviour_ButlerBot ||
						__instance is AI_NetworkBehaviour_MamaBear ||
						__instance is AI_NetworkBehaviour_Pig ||
						__instance is AI_NetworkBehaviour_PufferFish ||
						__instance is AI_NetworkBehaviour_Rat ||
						__instance is AI_NetworkBehaviour_StoneBird)
					{
						Minimap.AddMarker(__instance.transform, Minimap.MarkerType.ENEMY);
					}
					if (__instance is AI_NetworkBehaviour_BugSwarm ||
						__instance is AI_NetworkBehaviour_Chicken ||
						__instance is AI_NetworkBehaviour_Goat ||
						__instance is AI_NetworkBehaviour_Llama)
					{
						Minimap.AddMarker(__instance.transform, Minimap.MarkerType.NEUTRAL);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Network_Player), "Start")]
		class Start
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
