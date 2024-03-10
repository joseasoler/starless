using System.Collections.Generic;
using Verse;

namespace Starless.Extensions
{
	/// <summary>
	/// Determines the xenogenes of a pawn with this background. Compatible with both Childhood and Adulthood backgrounds.
	/// </summary>
	public class BackgroundXenogenes : DefModExtension
	{
		/// <summary>
		/// Genes to be added as xenogenes as part of the backstory.
		/// </summary>
		public List<GeneDef> xenogenes;

		public override IEnumerable<string> ConfigErrors()
		{
			if (xenogenes != null && xenogenes.Count == 0)
			{
				yield return Report.ConfigError(GetType(), $"{nameof(xenogenes)} must not be left empty if it is used.");
			}
		}
	}
}