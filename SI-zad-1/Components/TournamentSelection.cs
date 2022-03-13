using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal class TournamentSelection : ISelector
    {
        List<StationCost> Costs { get; }
        List<StationFlow> Flows { get; }
        int SelectionCount { get; }

        public TournamentSelection(List<StationCost> costs, List<StationFlow> flows, int selectionCount = 5)
        {
            Costs = costs;
            Flows = flows;
            SelectionCount = selectionCount;
        }

        public List<Specimen> Select(List<Specimen> specimens)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            for (int i = 0; i < specimens.Count; i++)
            {
                List<Specimen> specimensCopy = specimens.ToList();
                List<Specimen> selected = new List<Specimen>();
                for (int j = 0; j < SelectionCount; j++)
                {
                    selected.Add(new Specimen(specimensCopy[random.Next(specimensCopy.Count)]));
                    specimensCopy.Remove(selected[j]);
                }
                List<Specimen> bestSpecimens = selected.OrderBy((sp) => { return sp.SpecimenCost(Costs, Flows); })
                    .Take(1)
                    .ToList();
                result.AddRange(bestSpecimens);
            }
            return result;
        }
    }
}
