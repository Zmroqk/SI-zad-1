using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SI_zad_1
{
    internal class Loader
    {
        public static List<T>? LoadData<T>(string file)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(file));
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
    }
}
