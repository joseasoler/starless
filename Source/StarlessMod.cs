using Verse;

namespace Starless
{
	public class StarlessMod : Mod
	{
		public const string PackageId = "joseasoler.starless";
		public const string Name = "Starless";

		public StarlessMod(ModContentPack content) : base(content)
		{
			HarmonyHandler.Initialize();
			Report.Notice("Mod initialized.");
		}
	}
}