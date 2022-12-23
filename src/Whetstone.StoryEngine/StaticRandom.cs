using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Whetstone.StoryEngine
{
    public static class StaticRandom
    {


        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
#pragma warning disable SCS0005 // Weak random generator
            return random.Value.Next();
#pragma warning restore SCS0005 // Weak random generator
        }

        public static int Next(int min, int max)
        {
#pragma warning disable SCS0005 // Weak random generator
            return random.Value.Next(min, max);
#pragma warning restore SCS0005 // Weak random generator
        }

//        private static readonly Random globalRandom = new Random();
//        private static readonly object globalLock = new object();

//        public static Random NewRandom()
//        {
//            lock (globalLock)
//            {
//#pragma warning disable SCS0005 // Weak random generator
//                return new Random(globalRandom.Next());
//#pragma warning restore SCS0005 // Weak random generator

//            }
//        }


//        private static readonly ThreadLocal<Random> threadLocalRandom
//                    = new ThreadLocal<Random>(NewRandom);


//        public static Random Instance { get { return threadLocalRandom.Value; } }


    }
}
