using System;
using System.Collections.Generic;
using System.Runtime;

namespace KMeansPictureDifference
{
    class Vector
    {
        private int[] values;

        public Vector(int[] values)
        {
            this.values = values;
        }

        public static int vectorDistance(Vector vector1, Vector vector2)
        {
            int total = 0;
            for (int i = 0; i < vector1.getValues().Length; i++)
            {
                total += (int) Math.Pow(vector1.getValues()[i] - vector2.getValues()[i], 2);
            }

            return total;
        }

        public int[] getValues()
        {
            return values;
        }
    }
}
