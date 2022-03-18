using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal class RouletteSelection : ISelector
    {
        List<StationCost> Costs { get; }
        List<StationFlow> Flows { get; }

        public RouletteSelection(List<StationCost> costs, List<StationFlow> flows)
        {
            Costs = costs;
            Flows = flows;
        }

        public List<Specimen> Select(List<Specimen> specimens)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            List<(Specimen specimen, int fittness)> specimensWithCost = specimens.Select((sp) => (sp, sp.SpecimenCost(Costs, Flows))).ToList();
            int minValue = specimensWithCost.Min(sp => sp.fittness);
            int maxValue = specimensWithCost.Max(sp => sp.fittness);
            double weightConversion;
            if (minValue != maxValue)
            {
                double sumNormalized = specimensWithCost.Sum(sp => 1.2 - ((double)sp.fittness - minValue) / (maxValue - minValue));
                weightConversion = 1 / sumNormalized;
            }
            else
            {
                double sumNormalized = specimensWithCost.Sum(sp => sp.fittness);
                weightConversion = 1 / sumNormalized;
            }
            List<(double start, double end, Specimen specimen)> weightedSpecimens = new List<(double start, double end, Specimen specimen)>();
            double currentWeight = 0d;
            foreach ((Specimen specimen, int fittness) in specimensWithCost)
            {
                double normalizationValue = 1.2 - ((double)fittness - minValue) / (maxValue - minValue);
                if(normalizationValue == double.NaN)
                {
                    weightedSpecimens.Add((currentWeight, currentWeight + weightConversion, specimen));
                    currentWeight += weightConversion;
                }
                else
                {
                    weightedSpecimens.Add((currentWeight, currentWeight + normalizationValue * weightConversion, specimen));
                    currentWeight += normalizationValue * weightConversion;
                }
            }
            for (int i = 0; i < weightedSpecimens.Count; i++)
            {
                double value = random.NextDouble();
                Specimen specimen = weightedSpecimens.Find(wsp => wsp.start <= value && wsp.end > value).specimen;
                if (specimen != null)
                    result.Add(new Specimen(specimen));
                else
                    result.Add(new Specimen(weightedSpecimens[i].specimen));
            }
            return result;
        }
    }
}
