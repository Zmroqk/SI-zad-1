using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SI_zad_1.Components;

namespace SI_zad_1
{
    internal class LearningManager
    {
        public int Epoch { get; set; }
        public Specimen? BestSpecimen { get; private set; }
        public Specimen[] CurrentSpecimens { get; private set; }
        public List<(int epoch, Specimen[] specimens)> SpecimensArchive { get; private set; }
        public List<(int epoch, Specimen? specimen)> BestSpecimens { get; private set; } 
        List<StationCost> Costs { get; }
        List<StationFlow> Flows { get; }
        ISelector Selector { get; }
        IMutator Mutator { get; }
        ICrossover Crossover { get; }

        public LearningManager(List<StationCost> costs, List<StationFlow> flows, ISelector selector, IMutator mutator, ICrossover crossover)
        {
            Costs = costs;
            Flows = flows;
            Epoch = 1;
            BestSpecimens = new List<(int epoch, Specimen? specimen)>();
            SpecimensArchive = new List<(int epoch, Specimen[] specimens)>();
            CurrentSpecimens = new Specimen[0];
            BestSpecimen = null;
            Selector = selector;
            Mutator = mutator;
            Crossover = crossover;
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

        public void Evolve(
            double crossoverProbability = 0.5d, 
            double mutateProbability = 0.1d, 
            int elitizmCount = 5, 
            bool useElitizm = false)
        {
            List<Specimen> bestSpecimens = new List<Specimen>();
            if (useElitizm)
            {
                bestSpecimens = CurrentSpecimens.OrderBy(sp => sp.SpecimenCost(Costs, Flows)).Take(elitizmCount).ToList();
            }
            var selectedSpecimens = Selector.Select(CurrentSpecimens.ToList());
            var crossoverSpecimens = Crossover.Crossover(selectedSpecimens, crossoverProbability);     
            var mutatedSpecimens = Mutator.Mutate(crossoverSpecimens, mutateProbability);
            var newSpecimens = mutatedSpecimens;
            if(useElitizm)
            {
                newSpecimens = newSpecimens.OrderByDescending(sp => sp.SpecimenCost(Costs, Flows)).Skip(elitizmCount).ToList();
                newSpecimens.AddRange(bestSpecimens);
            }
            CurrentSpecimens = newSpecimens.ToArray();           
            Epoch++;
            SpecimensArchive.Add((Epoch, CurrentSpecimens));
            FindBestSpecimenInCurrentEpoch();
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
