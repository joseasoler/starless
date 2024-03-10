using HarmonyLib;
using Verse;

namespace Starless.HarmonyPatches
{
	[HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
	public static class PawnGenerator_GenerateGenes_Patch
	{
		public static bool Prefix(Pawn pawn)
		{
			// Vanilla gene generation will take place only if the backstory genes application failed.
			return !BackstoryGenesHandler.TryApply(pawn);
		}
	}
}