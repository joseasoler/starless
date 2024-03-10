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
		public List<XenotypeChance> firstXenotype;

		/// <summary>
		/// Second xenotype to use for setting endogenes. firstXenotype must be set. Generates a children of both xenotypes.
		/// </summary>
		public List<XenotypeChance> secondXenotype;

		public override IEnumerable<string> ConfigErrors()
		{
			if (firstXenotype.NullOrEmpty())
			{
				yield return Report.ConfigError(GetType(), $"{nameof(firstXenotype)} must have a value and cannot be empty.");
			}

			if (secondXenotype != null && secondXenotype.Count == 0)
			{
				yield return Report.ConfigError(GetType(), $"{nameof(secondXenotype)} must not be left empty if it is used.");
			}
		}
	}
}