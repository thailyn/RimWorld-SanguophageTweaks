using RimWorld;
using System.Text;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class StatPart_HemogenConcentration : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            float hemogenConcentration;
            if (!TryGetHemogenConcentration(req, out hemogenConcentration))
                return;
            val *= hemogenConcentration;
        }

        public override string ExplanationPart(StatRequest req)
        {
            float hemogenConcentration;
            var explanationBuilder = new StringBuilder();
            //if (!TryGetHemogenConcentration(req, out hemogenConcentration))
            //    return null;

            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return null;

            // For now, if a pawn does not have the hemogenic gene (that is, does not have the hemogen resource), the
            // pawn's hemogen concentration is always 1.
            if (!(pawn.genes?.GetGene(GeneDefOf.Hemogenic) is Gene_Hemogen hemogenGene))
            {
                hemogenConcentration = 1f;
                explanationBuilder.AppendLine($"Not hemogenic: x1");
                return explanationBuilder.ToString();
            }
            float hemogenValuePercent = hemogenGene.ValuePercent;

            float bloodPercent = 1f;
            Hediff bloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (!(bloodLossHediff is null))
                bloodPercent = 1f - bloodLossHediff.Severity;

            if (bloodPercent <= 0f)
            {
                hemogenConcentration = 0f;
                explanationBuilder.AppendLine("Blood loss at or greater than 100%: =0.00.");
            }
            else
            {
                hemogenConcentration = hemogenValuePercent / bloodPercent;
                explanationBuilder.AppendLine($"Hemogen value / blood percent: {100f * hemogenValuePercent:F2} / {bloodPercent:P2}");
            }
            return explanationBuilder.ToString();

            //return TryGetHemogenConcentration(req, out float hemogenConcentration) ?
            //    (string)("KOM.SanguophageTweaks.StatsReport_HemogenConcentration".Translate() + ": " +
            //    hemogenConcentration.ToString("F2")) : null;
        }

        private bool TryGetHemogenConcentration(StatRequest req, out float hemogenConcentration)
        {
            //hemogenConcentration = 0f;
            hemogenConcentration = 123f;
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return false;

            // For now, if a pawn does not have the hemogenic gene (that is, does not have the hemogen resource), the
            // pawn's hemogen concentration is always 1.
            if (!(pawn.genes?.GetGene(GeneDefOf.Hemogenic) is Gene_Hemogen hemogenGene))
            {
                hemogenConcentration = 1f;
                //hemogenConcentration = 2f;  // (for testing)
                return true;
            }
            float hemogenValuePercent = hemogenGene.ValuePercent;

            float bloodPercent = 1f;
            Hediff bloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (!(bloodLossHediff is null))
            {
                bloodPercent -= bloodLossHediff.Severity;
                //hemogenConcentration = 3f;
                //return false;
            }


            //float bloodPercent = 1f - bloodLossHediff.Severity;
            if (bloodPercent <= 0f)
                hemogenConcentration = 0f;
            else
                hemogenConcentration = hemogenValuePercent / bloodPercent;

            return true;
        }
    }
}
