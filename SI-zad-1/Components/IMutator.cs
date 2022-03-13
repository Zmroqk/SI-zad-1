using SI_zad_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Components
{
    internal interface IMutator
    {
        public List<Specimen> Mutate(List<Specimen> specimens, double probability = 0.1);
    }
}
