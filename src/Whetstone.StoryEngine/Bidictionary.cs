using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine
{
    public class BiDictionary<TFirst, TSecond> 
    {
        private readonly IDictionary<TFirst, IList<TSecond>> firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
        private readonly IDictionary<TSecond, IList<TFirst>> secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

        private static readonly IList<TFirst> EmptyFirstList = new TFirst[0];
        private static readonly IList<TSecond> EmptySecondList = new TSecond[0];

        public void Add(TFirst first, TSecond second)
        {
            if (!firstToSecond.TryGetValue(first, out IList<TSecond> seconds))
            {
                seconds = new List<TSecond>();
                firstToSecond[first] = seconds;
            }
            if (!secondToFirst.TryGetValue(second, out IList<TFirst> firsts))
            {
                firsts = new List<TFirst>();
                secondToFirst[second] = firsts;
            }
            seconds.Add(second);
            firsts.Add(first);
        }

        // Note potential ambiguity using indexers (e.g. mapping from int to int)
        // Hence the methods as well...
        public IList<TSecond> this[TFirst first]
        {
            get { return GetByFirst(first); }
        }

        public IList<TFirst> this[TSecond second]
        {
            get { return GetBySecond(second); }
        }

        public IList<TSecond> GetByFirst(TFirst first)
        {
            if (!firstToSecond.TryGetValue(first, out IList<TSecond> list))
            {
                return EmptySecondList;
            }
            return new List<TSecond>(list); // Create a copy for sanity
        }

        public IList<TFirst> GetBySecond(TSecond second)
        {
            if (!secondToFirst.TryGetValue(second, out IList<TFirst> list))
            {
                return EmptyFirstList;
            }
            return new List<TFirst>(list); // Create a copy for sanity
        }
    }
}
