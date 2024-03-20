using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Starless.Extensions;
using Verse;

namespace Starless
{
	public static class BackstoryGenesHandler
	{
		private static float XenotypeChanceWeight(XenotypeChance xenotypeChance)
		{
			return xenotypeChance.chance;
		}

		/// <summary>
		/// Get a random xenotype from a list of xenotype chances.
		/// </summary>
		/// <param name="xenotypeChances">Weighted xenotypes to use.</param>
		/// <param name="excludedDef">Xenotype to be excluded from potential results.</param>
		/// <returns>Random xenotype definition, or null if it was not possible to find one.</returns>
		private static XenotypeDef GetRandomXenotype(List<XenotypeChance> xenotypeChances, XenotypeDef excludedDef = null)
		{
			if (xenotypeChances.NullOrEmpty())
			{
				return null;
			}

			if (excludedDef != null)
			{
				List<XenotypeChance> chances = new List<XenotypeChance>();
				for (int index = 0; index < xenotypeChances.Count; ++index)
				{
					XenotypeChance chance = xenotypeChances[index];
					if (chance.xenotype != excludedDef)
					{
						chances.Add(chance);
					}
				}

				xenotypeChances = chances;
			}


			return xenotypeChances.TryRandomElementByWeight(XenotypeChanceWeight, out XenotypeChance firstXenotype)
				? firstXenotype.xenotype
				: null;
		}

		/// <summary>
		/// Generates a dummy pawn initializing only the fields strictly necessary to use it as a parent.
		/// This is an unfortunate requirement of relying on the PregnancyUtility.GetInheritedGeneSet vanilla function,
		/// as it only supports fully fledged pawns and it cannot work with XenotypeDefs. To avoid the duplication of
		/// vanilla code (and probably losing mod compatibility too), this solution is used instead.
		/// </summary>
		/// <param name="xenotypeDef">Xenotype of the parent.</param>
		/// <returns></returns>
		private static Pawn GetDummyParent(XenotypeDef xenotypeDef)
		{
			Pawn parent = new Pawn();
			parent.genes = new Pawn_GeneTracker();
			parent.genes.xenotype = xenotypeDef;
			foreach (GeneDef geneDef in xenotypeDef.genes)
			{
				parent.genes.Endogenes.Add(GeneMaker.MakeGene(geneDef, parent));
			}

			parent.relations = new Pawn_RelationsTracker(parent);
			parent.def = ThingDefOf.Human;

			return parent;
		}

		/// <summary>
		/// Set the pawn to have a hybrid xenotype between the two provided xenotypes.
		/// This function mimics parts of the vanilla code, as mentioned in the comments.
		/// </summary>
		/// <param name="child">Pawn to be modified.</param>
		/// <param name="motherXenotypeDef">Xenotype of the mother.</param>
		/// <param name="fatherXenotypeDef">Xenotype of the father. Must not be the same as motherXenotypeDef.</param>
		private static void SetToHybrid(Pawn child, XenotypeDef motherXenotypeDef, XenotypeDef fatherXenotypeDef)
		{
			child.genes.ClearXenogenes();

			Pawn mother = GetDummyParent(motherXenotypeDef);
			Pawn father = GetDummyParent(fatherXenotypeDef);

			// See HumanEmbryo.TryPopulateGenes_NewTemp.
			GeneSet genes = PregnancyUtility.GetInheritedGeneSet(mother, father);
			foreach (GeneDef gene in genes.GenesListForReading)
			{
				child.genes.AddGene(gene, false);
			}

			// See the part of PregnancyUtility.ApplyBirthOutcome related to determining the child's xenotype.
			child.genes.hybrid = true;
			child.genes.xenotypeName = "Hybrid".Translate();
		}

		/// <summary>
		/// Applies additional genes coming from the background of the pawn.
		/// </summary>
		/// <param name="pawn">Pawn to be modified.</param>
		/// <param name="genes">Genes to apply.</param>
		/// <param name="asXenogenes">True if the gene should be added as a xenogene.</param>
		/// <returns>True if any changes were made.</returns>
		private static bool TryAddGenes(Pawn pawn, List<GeneDef> genes, bool asXenogenes)
		{
			if (genes.NullOrEmpty())
			{
				return false;
			}

			foreach (GeneDef gene in genes)
			{
				pawn.genes.AddGene(gene, asXenogenes);
			}

			return true;
		}

		/// <summary>
		/// Applies additional xenogenes coming from the background of the pawn.
		/// </summary>
		/// <param name="pawn">Pawn to be modified.</param>
		/// <param name="backgroundXenogenes">Xenogenes to apply.</param>
		/// <returns>True if any changes were made.</returns>
		private static bool TryAddXenogenes(Pawn pawn, BackgroundXenogenes backgroundXenogenes)
		{
			if (backgroundXenogenes == null)
			{
				return false;
			}

			return TryAddGenes(pawn, backgroundXenogenes.xenogenes, true);
		}

		/// <summary>
		/// Applies gene changes caused by the childhood backstory of the pawn. These can come from the BackgroundEndogenes
		/// and BackgroundXenogenes mod extensions.
		/// </summary>
		/// <param name="pawn">Pawn being generated.</param>
		/// <returns>True if any modifications took place.</returns>
		private static bool TrySetChildhoodGenes(Pawn pawn)
		{
			BackstoryDef childhoodBackstoryDef = pawn.story.Childhood;
			if (childhoodBackstoryDef == null || childhoodBackstoryDef.modExtensions.NullOrEmpty())
			{
				return false;
			}

			bool madeChanges = false;
			BackgroundEndogenes endogenes = null;
			BackgroundXenogenes xenogenes = null;

			foreach (DefModExtension modExtension in childhoodBackstoryDef.modExtensions)
			{
				if (endogenes == null && modExtension is BackgroundEndogenes backgroundEndogenes)
				{
					endogenes = backgroundEndogenes;
				}
				else if (xenogenes == null && modExtension is BackgroundXenogenes backgroundXenogenes)
				{
					xenogenes = backgroundXenogenes;
				}
			}

			if (endogenes != null)
			{
				XenotypeDef firstXenotypeDef = GetRandomXenotype(endogenes.xenotype?.xenotypes);
				if (firstXenotypeDef != null)
				{
					XenotypeDef secondXenotypeDef = GetRandomXenotype(endogenes.hybridXenotype?.xenotypes, firstXenotypeDef);
					madeChanges = true;

					if (secondXenotypeDef == null)
					{
						pawn.genes.SetXenotype(firstXenotypeDef);
					}
					else
					{
						SetToHybrid(pawn, firstXenotypeDef, secondXenotypeDef);
					}
				}

				// Forced endogenes are only applied when both xenotype application and hybridization did not take place.
				madeChanges = madeChanges || TryAddGenes(pawn, endogenes.endogenes, false);

				// Inbred chance is always applied.
				if (Rand.Value <= endogenes.inbredChance)
				{
					madeChanges = true;
					pawn.genes.AddGene(GeneDefOf.Inbred, false);
				}
			}

			madeChanges = TryAddXenogenes(pawn, xenogenes) || madeChanges;

			return madeChanges;
		}

		/// <summary>
		/// Applies gene changes caused by the adulthood backstory of the pawn. These can come from the BackgroundXenogenes
		/// mod extension.
		/// </summary>
		/// <param name="pawn">Pawn being generated.</param>
		/// <returns>True if any modifications took place.</returns>
		private static bool TrySetAdulthoodGenes(Pawn pawn)
		{
			BackgroundXenogenes xenogenes = pawn.story.Adulthood?.GetModExtension<BackgroundXenogenes>();
			return TryAddXenogenes(pawn, xenogenes);
		}

		/// <summary>
		/// Apply genes coming from backstories to a pawn.
		/// </summary>
		/// <param name="pawn">Pawn to be modified</param>
		/// <returns>True if the Starless code is taking control over gene generation for this pawn.</returns>
		public static bool TryApply(Pawn pawn)
		{
			if (pawn.genes == null || !ModsConfig.BiotechActive || pawn.story == null)
			{
				return false;
			}

			bool madeChanges = TrySetChildhoodGenes(pawn);
			madeChanges = TrySetAdulthoodGenes(pawn) || madeChanges;
			return madeChanges;
		}
	}
}