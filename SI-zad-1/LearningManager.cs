using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1
{
    internal class LearningManager
    {
        public int Epoch { get; set; }
        Specimen[] Specimens { get; set; }
        List<StationCost> Costs { get; set; }
        List<StationFlow> Flows { get; set; }

        public LearningManager(List<StationCost> costs, List<StationFlow> flows)
        {
            Costs = costs;
            Flows = flows;
            Epoch = 1;
        }

        public void Init(int specimenCount, int w, int h, int n)
        {
            Specimens = new Specimen[specimenCount];
            for (int i = 0; i < specimenCount; i++)
            {
                Specimens[i] = new Specimen(w, h);
                Specimens[i].GenerateRandomSpecimen(n);
            }
        }

        public void Evolve(SelectionMethod method, double crossoverProbability = 0.5d, double mutateProbability = 0.1d, int selectionCount = 5)
        {
            var selectedSpecimens =
                method == SelectionMethod.Tournament ? TournamentSelection(selectionCount) : RouletteSelection();
            var crossoverSpecimens = Crossover(selectedSpecimens, crossoverProbability);
            var mutatedSpecimens = Mutate(crossoverSpecimens, mutateProbability);
            Specimens = mutatedSpecimens.ToArray();
            Epoch++;
        }

        List<Specimen> TournamentSelection(int randomSelectionCount)
        {
            Random random = new Random();         
            List<Specimen> result = new List<Specimen>();
            for(int i = 0; i < Specimens.Length; i++)
            {
                List<Specimen> specimensCopy = Specimens.ToList();
                List<Specimen> selected = new List<Specimen>();
                for(int j = 0; j < randomSelectionCount; j++)
                {
                    selected.Add(new Specimen(specimensCopy[random.Next(specimensCopy.Count)]));
                    specimensCopy.Remove(selected[j]);
                }
                List<Specimen> bestSpecimens = selected.OrderByDescending((sp) => { return sp.SpecimenCost(Costs, Flows); })
                    .Take(1)
                    .ToList();
                result.AddRange(bestSpecimens);
            }
            return result;
        }

        List<Specimen> RouletteSelection()
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            List<(Specimen specimen, int fittness)> specimensWithCost = Specimens.Select((sp) => (sp, sp.SpecimenCost(Costs, Flows))).ToList();
            int specimensPoints = specimensWithCost.Sum(sp => sp.fittness);
            double conversionRatio = 1d / specimensPoints;
            List<(double start, double end, Specimen specimen)> weightedSpecimens = new List<(double start, double end, Specimen specimen)>();
            double currentWeight = 0d;
            foreach((Specimen specimen, int fittness) in specimensWithCost)
            {
                weightedSpecimens.Add((currentWeight, currentWeight + fittness * conversionRatio, specimen));
                currentWeight += fittness * conversionRatio;
            }
            for(int i = 0; i < weightedSpecimens.Count; i++)
            {
                double value = random.NextDouble();
                result.Add(weightedSpecimens.Find(wsp => wsp.start <= value && wsp.end > value).specimen);
            }
            return result;
        }

        List<Specimen> Crossover(List<Specimen> selectedSpecimens, double probability = 0.5d)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            for(int i = 0; i < selectedSpecimens.Count; i++)
            {
                if(random.NextDouble() <= probability)
                {
                    int secondSpecimenIndex = i + 1;
                    if(secondSpecimenIndex >= selectedSpecimens.Count)
                    {
                        secondSpecimenIndex = random.Next(selectedSpecimens.Count);
                    }
                    Specimen first = new Specimen(selectedSpecimens[i++]);
                    Specimen second = new Specimen(selectedSpecimens[secondSpecimenIndex]);
                    OnePointCrossover(first, second);
                    result.Add(first);
                    if(result.Count < selectedSpecimens.Count)
                        result.Add(second);
                }
                else
                {
                    result.Add(new Specimen(selectedSpecimens[i]));
                }
            }
            return result;
        }

        List<Specimen> Mutate(List<Specimen> selectedSpecimens, double probability = 0.1d)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            for(int i = 0; i < selectedSpecimens.Count; i++)
            {
                if(random.NextDouble() <= probability)
                {
                    Specimen mutatedSpecimen = new Specimen(selectedSpecimens[i]);
                    int mutateIndex = random.Next(mutatedSpecimen.Stations.Count);
                    List<(int x, int y)> coords = mutatedSpecimen.Stations.Select(st => (st.coord.X, st.coord.Y)).ToList();
                    (int x, int y) newCoords;
                    do
                    {
                        int position = random.Next(mutatedSpecimen.H * mutatedSpecimen.W);
                        newCoords = (position / mutatedSpecimen.H, position % mutatedSpecimen.W);
                    }
                    while (!coords.Contains(newCoords));
                    mutatedSpecimen.Stations[mutateIndex] = (mutatedSpecimen.Stations[mutateIndex].index, new Coordinates(newCoords.x, newCoords.y));
                    result.Add(mutatedSpecimen);
                }
                else
                {
                    result.Add(new Specimen(selectedSpecimens[i]));
                }
            }
            return result;
        }

        void OnePointCrossover(Specimen first, Specimen second)
        {
            Random random = new Random();
            int splitIndex = random.Next(first.Stations.Count);
            List<(int, Coordinates)> firstStations = first.Stations.Take(splitIndex + 1).ToList();
            List<(int, Coordinates)> firstRestStations = first.Stations.TakeLast(first.Stations.Count - splitIndex - 1).ToList();
            List<(int, Coordinates)> secondStations = second.Stations.Take(splitIndex + 1).ToList();
            List<(int, Coordinates)> secondRestStations = second.Stations.TakeLast(first.Stations.Count - splitIndex - 1).ToList();
            first.Stations.Clear();
            second.Stations.Clear();
            first.Stations.AddRange(secondStations);
            first.Stations.AddRange(firstRestStations);
            second.Stations.AddRange(firstStations);
            second.Stations.AddRange(secondRestStations);

            FixSpecimen(first);
            FixSpecimen(second);    
        }

        void FixSpecimen(Specimen specimen)
        {
            Random random = new Random();
            Dictionary<int, int> indexes = new Dictionary<int, int>();
            Dictionary<Coordinates, int> coords = new Dictionary<Coordinates, int>();
            for (int i = 0; i < specimen.Stations.Count; i++)
            {
                if (!indexes.ContainsKey(i))
                    indexes.Add(i, 0);
                (int index, Coordinates coord) = specimen.Stations[i];
                if (!indexes.ContainsKey(index))
                    indexes.Add(index, 1);
                else
                    indexes[index]++;
                if (!coords.ContainsKey(new Coordinates(coord.X, coord.Y)))
                    coords.Add(new Coordinates(coord.X, coord.Y), 1);
                else
                    coords[new Coordinates(coord.X, coord.Y)]++;
            }
            var duplicateIndexes = indexes.Where(pair => pair.Value >= 2).ToList();
            if (duplicateIndexes.Count > 0)
            {
                var missingIndexes = indexes.Where(pair => pair.Value == 0).ToList();
                foreach (var duplicate in duplicateIndexes)
                {
                    int replaceIndex = specimen.Stations.FindIndex(st => st.index == duplicate.Key);
                    specimen.Stations[replaceIndex] = (
                        missingIndexes[0].Key,
                        new Coordinates(
                            specimen.Stations[replaceIndex].coord.X,
                            specimen.Stations[replaceIndex].coord.Y
                            )
                        );
                    missingIndexes.RemoveAt(0);
                }
            }
            var duplicateCoords = coords.Where(pair => pair.Value >= 2).ToList();
            if (duplicateCoords.Count > 0)
            {
                foreach (var duplicate in duplicateCoords)
                {
                    int replaceIndex = specimen.Stations.FindIndex(
                        st => st.coord.X == duplicate.Key.X && st.coord.Y == duplicate.Key.Y
                        );
                    Coordinates newCoords;
                    do
                    {
                        int position = random.Next(specimen.H * specimen.W);
                        newCoords = new Coordinates(position / specimen.H, position % specimen.W);
                    }
                    while (!coords.ContainsKey(newCoords));
                    specimen.Stations[replaceIndex] = (specimen.Stations[replaceIndex].index, new Coordinates(newCoords.X, newCoords.Y));
                    coords.Add(newCoords, 1);
                }
            }
        }

        public override string ToString()
        {
            int[] costs = new int[Specimens.Length];
            for(int i = 0; i < costs.Length; i++)
            {
                costs[i] = Specimens[i].SpecimenCost(Costs, Flows);
            }
            return $"[{string.Join(',', costs)}]";
        }

        internal enum SelectionMethod
        {
            Tournament,
            Roulette
        }
    }
}
