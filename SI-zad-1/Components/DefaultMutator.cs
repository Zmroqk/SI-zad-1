using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal class DefaultMutator : IMutator
    {
        public List<Specimen> Mutate(List<Specimen> specimens, double probability = 0.1d)
        {
            Random random = new Random();
            List<Specimen> result = new List<Specimen>();
            for (int i = 0; i < specimens.Count; i++)
            {
                if (random.NextDouble() <= probability)
                {
                    Specimen mutatedSpecimen = new Specimen(specimens[i]);
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
                    result.Add(new Specimen(specimens[i]));
                }
            }
            return result;
        }
    }
}
