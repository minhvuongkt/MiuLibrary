using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiuLibrary.Constants
{
    public class MyRandom
    {
        public Random R;

        public MyRandom()
        {
            R = new Random();
        }

        public int NextInt()
        {
            return R.Next();
        }

        public int NextInt(int a)
        {
            if (a <= 0) return 0;
            return R.Next(a);
        }

        public int NextInt(int a, int b)
        {
            return R.Next(a, b);
        }
    }
}
