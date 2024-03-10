using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Starless.Defs
{
	/// <summary>
	/// Utility Def for creating a list of weighted xenotypes that can be shared by other Defs.
	/// </summary>
	public class XenotypeChancesDef : Def
	{
		public List<XenotypeChance> xenotypes;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string error in base.ConfigErrors())
			{
				yield return error;
			}

			if (xenotypes.NullOrEmpty())
			{
				yield return Report.ConfigError(GetType(), $"must define a {nameof(xenotypes)} list with one or more values.");
			}
		}
	}
}