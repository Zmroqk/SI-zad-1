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
        public Specimen? BestSpecimen { get; private set; }
        public Specimen[] CurrentSpecimens { get; private set; }
        public List<(int epoch, Specimen[] specimens)> SpecimensArchive { get; private set; }
        public List<(int epoch, Specimen? specimen)> BestSpecimens { get; private set; } 
        List<StationCost> Costs { get; set; }
        List<StationFlow> Flows { get; set; }

        public LearningManager(List<StationCost> costs, List<StationFlow> flows)
        {
            Costs = costs;
            Flows = flows;
            Epoch = 1;
            BestSpecimens = new List<(int epoch, Specimen? specimen)>();
            SpecimensArchive = new List<(int epoch, Specimen[] specimens)>();
            CurrentSpecimens = new Specimen[0];
            BestSpecimen = null;
        }

        public void Init(int specimenCount, int w, int h, int n)
        {
            CurrentSpecimens = new Specimen[specimenCount];
            for (int i = 0; i < specimenCount; i++)
            {
                CurrentSpecimens[i] = new Specimen(w, h);
                CurrentSpecimens[i].GenerateRandomSpecimen(n);
            }
            FindBestSpecimenInCurrentEpoch();
            SpecimensArchive.Add((Epoch, CurrentSpecimens));
        }

        public void Evolve(SelectionMethod method, double crossoverProbability = 0.5d, double mutateProbability = 0.1d, int selectionCount = 5)
        {
            var selectedSpecimens =
                method == SelectionMethod.Tournament ? TournamentSelection(selectionCount) : RouletteSelection();
            var crossoverSpecimens = Crossover(selectedSpecimens, crossoverProbability);     
            var mutatedSpecimens = Mutate(crossoverSpecimens, mutateProbability);
            CurrentSpecimens = mutatedSpecimens.ToArray();           
            Epoch++;
            SpecimensArchive.Add((Epoch, CurrentSpecimens));
            FindBestSpecimenInCurrentEpoch();
        }

        List<Specimen> TournamentSelection(int randomSelectionCount)
        {
            Random random = new Random();         
            List<Specimen> result = new List<Specimen>();
            for(int i = 0; i < CurrentSpecimens.Length; i++)
            {
                List<Specimen> specimensCopy = CurrentSpecimens.ToList();
                List<Specimen> selected = new List<Specimen>();
                for(int j = 0; j < randomSelectionCount; j++)
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

        List<Specimen> RouletteSelection()
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            List<(Specimen specimen, int fittness)> specimensWithCost = CurrentSpecimens.Select((sp) => (sp, sp.SpecimenCost(Costs, Flows))).ToList();
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
            foreach((Specimen specimen, int fittness) in specimensWithCost)
            {
                double normalizationValue = 1.2 - ((double)fittness - minValue) / (maxValue - minValue);
                weightedSpecimens.Add((currentWeight, currentWeight + (1.2 - ((double)fittness - minValue) / (maxValue - minValue)) * weightConversion, specimen));
                currentWeight += (1 - ((double)fittness - minValue) / (maxValue - minValue)) * weightConversion;
            }
            for(int i = 0; i < weightedSpecimens.Count; i++)
            {
                double value = random.NextDouble();
                Specimen specimen = weightedSpecimens.Find(wsp => wsp.start <= value && wsp.end > value).specimen;
                if (specimen != null)
                    result.Add(specimen);
                else
                    result.Add(weightedSpecimens[i].specimen);
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
                    List<Coordinates> coords = mutatedSpecimen.Stations.Select(st => st.coord).ToList();
                    Coordinates newCoords;
                    bool flag = false;
                    int it = 0;
                    do
                    {
                        int position = random.Next(mutatedSpecimen.H * mutatedSpecimen.W);
                        newCoords = new Coordinates(position / mutatedSpecimen.H, position % mutatedSpecimen.W);
                        flag = coords.Contains(newCoords);
                        it++;
                    }
                    while (flag && it < 5);
                    if (flag)
                    {
                        int firstStation = random.Next(mutatedSpecimen.Stations.Count);
                        int secondStation;
                        do
                        {
                            secondStation = random.Next(mutatedSpecimen.Stations.Count);
                        } while (firstStation == secondStation);
                        mutatedSpecimen.Stations[firstStation] = (
                            mutatedSpecimen.Stations[secondStation].index,
                            mutatedSpecimen.Stations[firstStation].coord
                        );
                        mutatedSpecimen.Stations[secondStation] = (
                            mutatedSpecimen.Stations[firstStation].index,
                            mutatedSpecimen.Stations[secondStation].coord
                        );
                    }
                    else
                        mutatedSpecimen.Stations[mutateIndex] = (mutatedSpecimen.Stations[mutateIndex].index, new Coordinates(newCoords.X, newCoords.Y));             
                    result.Add(mutatedSpecimen);
                }
                else
                {
                    result.Add(new Specimen(selectedSpecimens[i]));
                }
            }
            return result;
        }

        void FindBestSpecimenInCurrentEpoch()
        {
            Specimen? bestSpecimen = CurrentSpecimens.MinBy(sp => sp.SpecimenCost(Costs, Flows));
            BestSpecimens.Add((Epoch, bestSpecimen));
            if(BestSpecimen != null && bestSpecimen != null && BestSpecimen.SpecimenCost(Costs, Flows) > bestSpecimen.SpecimenCost(Costs, Flows))
            {
                BestSpecimen = bestSpecimen;
            }
            else if(BestSpecimen == null && bestSpecimen != null)
            {
                BestSpecimen = bestSpecimen;
            }
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
                List<Coordinates> missingCoordinates = new List<Coordinates>();
                for(int i = 0; i < specimen.W; i++)
                {
                    for(int j = 0; j < specimen.H; j++)
                    {
                        missingCoordinates.Add(new Coordinates(i, j));
                    }
                }
                foreach(var coord in coords)
                {
                    missingCoordinates.Remove(coord.Key);
                }             
                foreach (var duplicate in duplicateCoords)
                {
                    for(int i = 0; i < duplicate.Value-1; i++)
                    {
                        int replaceIndex = specimen.Stations.FindIndex(
                            st => st.coord.X == duplicate.Key.X && st.coord.Y == duplicate.Key.Y
                            );
                        int position = random.Next(missingCoordinates.Count);
                        Coordinates newCoords = missingCoordinates[position];
                        specimen.Stations[replaceIndex] = (specimen.Stations[replaceIndex].index, newCoords);
                        missingCoordinates.RemoveAt(position);
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var bestSpecimen in BestSpecimens)
            {
                stringBuilder.AppendLine($"Epoch: {bestSpecimen.epoch} Best score: {bestSpecimen.specimen?.SpecimenCost(Costs, Flows)}");
            }
            return stringBuilder.ToString();
        }

        public string ToString(int epoch)
        {
            var bestInEpoch = BestSpecimens.Find(sp => sp.epoch == epoch);
            return $"Epoch: {bestInEpoch.epoch} Best score: {bestInEpoch.specimen?.SpecimenCost(Costs, Flows)}";
        }

        public string ToStringBestSpecimen()
        {
            return $"Best specimen cost: {BestSpecimen?.SpecimenCost(Costs, Flows)}\nFull details: {BestSpecimen}";
        }

        internal enum SelectionMethod
        {
            Tournament,
            Roulette
        }
    }
}
