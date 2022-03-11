using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SI_zad_1.Models
{
    internal class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            if(obj != null && obj is Coordinates)
            {
                Coordinates coord = (Coordinates)obj;
                return this.X == coord.X && this.Y == coord.Y;
            }
            return false;
        }

        public override string ToString()
        {
            return $"(X: {X} Y: {Y})";
        }

        /// <summary>
        /// Bijective algorithm HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int tmp = (Y + ((X + 1) / 2));
            return X + (tmp * tmp);
        }
    }
}
