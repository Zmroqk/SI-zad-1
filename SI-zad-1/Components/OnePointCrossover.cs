using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal class OnePointCrossover : ICrossover
    {
        public List<Specimen> Crossover(List<Specimen> selectedSpecimens, double probability = 0.5)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            for (int i = 0; i < selectedSpecimens.Count; i++)
            {
                if (random.NextDouble() <= probability)
                {
                    int secondSpecimenIndex = i + 1;
                    if (secondSpecimenIndex >= selectedSpecimens.Count)
                    {
                        secondSpecimenIndex = random.Next(selectedSpecimens.Count);
                    }
                    Specimen first = new Specimen(selectedSpecimens[i++]);
                    Specimen second = new Specimen(selectedSpecimens[secondSpecimenIndex]);
                    CrossoverSpecimens(first, second);
                    result.Add(first);
                    if (result.Count < selectedSpecimens.Count)
                        result.Add(second);
                }
                else
                {
                    result.Add(new Specimen(selectedSpecimens[i]));
                }
            }
            return result;
        }

        void CrossoverSpecimens(Specimen first, Specimen second)
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
                for (int i = 0; i < specimen.W; i++)
                {
                    for (int j = 0; j < specimen.H; j++)
                    {
                        missingCoordinates.Add(new Coordinates(i, j));
                    }
                }
                foreach (var coord in coords)
                {
                    missingCoordinates.Remove(coord.Key);
                }
                foreach (var duplicate in duplicateCoords)
                {
                    for (int i = 0; i < duplicate.Value - 1; i++)
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
    }
}
