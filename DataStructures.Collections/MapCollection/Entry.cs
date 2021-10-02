using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.Collections.Map
{
    /// <summary>
    /// Entry that holds data in Map
    /// </summary>
    internal struct Entry<TKey, TValue>
    {
        public int hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }
}
