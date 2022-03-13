using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Models
{
    internal class Specimen
    {
        public int W { get; set; }
        public int H { get; set; }
        public List<(int index, Coordinates coord)> Stations { get; private set; }

        int? Cost { get; set; }

        public Specimen(int w, int h)
        {
            W = w;
            H = h;
            Stations = new List<(int, Coordinates)>();
        }

        public Specimen(Specimen specimen)
        {
            W = specimen.W;
            H = specimen.H;
            Stations = new List<(int, Coordinates)>(specimen.Stations);
        }

        public void GenerateRandomSpecimen(int n)
        {
            int length = W*H;
            if (n > length)
            {
                throw new ArgumentException("N value cannot be bigger than matrix length");
            }
            List<int> positionsTaken = new List<int>();
            int i = 0;
            Random random = new Random();
            while(i < n)
            {
                int position = random.Next(0, length);
                if (!positionsTaken.Contains(position)) {
                    positionsTaken.Add(position);
                    i++;
                }
            }
            for(int j = 0; j < positionsTaken.Count; j++)
            {
                Stations.Add((j, new Coordinates(positionsTaken[j] / H, positionsTaken[j] % H)));
            }
        }

        public int SpecimenCost(List<StationCost> stationCosts, List<StationFlow> stationFlows)
        {
            if(Cost == null)
            {
                int fitness = 0;
                int stationIndex = 0;
                foreach ((int index, Coordinates coord) station in Stations)
                {
                    List<StationCost> costs = stationCosts.FindAll((sCost) => sCost.Source == stationIndex);
                    List<StationFlow> flows = stationFlows.FindAll((sFlow) => sFlow.Source == stationIndex);
                    foreach (var cost in costs)
                    {
                        StationFlow? flow = flows.Find(flow => flow.Dest == cost.Dest);
                        (int index, Coordinates coord) destCoords = Stations[cost.Dest];
                        if (flow != null)
                            fitness += cost.Cost * flow.Amount * (
                                Math.Abs(station.coord.X - destCoords.coord.X) + Math.Abs(station.coord.Y - destCoords.coord.Y)
                                );
                        else
                            throw new Exception("Provided data are incorrect");
                    }
                    stationIndex++;
                }
                Cost = fitness;
                return fitness;
            }
            else
            {
                return (int)Cost;
            }
        }

        public override string ToString()
        {
            string stations = string.Join<Coordinates>(',', Stations.Select(s => s.coord).ToArray());
            return $"Specimen: ({W},{H}) Cost: {Cost} Stations: {stations}";
        }
    }
}
