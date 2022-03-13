using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal interface ICrossover
    {
        public  List<Specimen> Crossover(List<Specimen> selectedSpecimens, double probability = 0.5d);
    }
}
