using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Starless.Extensions
{
	/// <summary>
	/// Determines the endogenes of a pawn with a specific childhood background. This extension does not have effect on
	/// adulthood backgrounds.
	/// </summary>
	public class BackgroundEndogenes : DefModExtension
	{
		/// <summary>
		/// First xenotype to use for setting endogenes. If secondXenotype is not set, then this will be the final xenotype.
		/// </summary>
		public Defs.XenotypeChancesDef xenotype;

		/// <summary>
		/// Second xenotype to use for setting endogenes. xenotype must be set. Generates a hybrid of both xenotypes.
		/// </summary>
		public Defs.XenotypeChancesDef hybridXenotype;

		public override IEnumerable<string> ConfigErrors()
		{
			if (xenotype == null)
			{
				yield return Report.ConfigError(GetType(), $"{nameof(xenotype)} must have a value.");
			}
		}
	}
}