using System;

namespace Starless
{
	/// <summary>
	/// Handles the application of harmony patches.
	/// </summary>
	public static class HarmonyHandler
	{
		private static HarmonyLib.Harmony _harmonyInstance;

		/// <summary>
		/// Apply harmony patches to the game.
		/// </summary>
		public static void Initialize()
		{
			try
			{
				_harmonyInstance = new HarmonyLib.Harmony(StarlessMod.PackageId);
				_harmonyInstance.PatchAll();
			}
			catch (Exception exception)
			{
				Report.Error("Harmony patching failed:");
				Report.Error($"{exception}");
			}
		}
	}
}