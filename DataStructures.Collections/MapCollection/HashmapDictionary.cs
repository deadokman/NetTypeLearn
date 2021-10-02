using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.Collections.Map
{
    /// <summary>
    /// Map for key/value pairs based on hash table alg
    /// </summary>
    public class HashmapDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// The constant 0x7FFFFFFF is a 32-bit integer in hexadecimal with all but the highest bit set.
        /// </summary>
        private const int NonNegativeInt32Mask = 0x7FFFFFFF;

        private int[] _buckets;

        private Entry<TKey, TValue>[] _entries;

        private int _freeList;

        private IEqualityComparer<TKey> _equalityComparer;

        private int _version;

        // Count of vacant places after deleting the key
        private int _freeCount;

        private int _count;

        /// <summary>
        /// Default map constructor with zero capacity
        /// </summary>
        public HashmapDictionary()
            : this(0)
        {
        }

        /// <summary>
        /// Creates map with given capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public HashmapDictionary(int capacity, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity > 0)
            {
                Initialize(capacity);
            }

            _equalityComparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        /// <inheritdoc/>
        public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ICollection<TKey> Keys => throw new NotImplementedException();
        
        /// <inheritdoc/>
        public ICollection<TValue> Values => throw new NotImplementedException();
        
        /// <inheritdoc/>
        public int Count => throw new NotImplementedException();
        
        /// <inheritdoc/>
        public bool IsReadOnly => throw new NotImplementedException();
        
        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }
        
        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public void Clear()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_buckets != null)
            {
                int hashCode = _equalityComparer.GetHashCode(key) & NonNegativeInt32Mask;
                int bucket = hashCode % _buckets.Length;
                int last = -1;
                for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].next)
                {
                    if (_entries[i].hashCode == hashCode && _equalityComparer.Equals(_entries[i].key, key))
                    {
                        if (last < 0)
                        {
                            _buckets[bucket] = _entries[i].next;
                        }
                        else
                        {
                            _entries[last].next = _entries[i].next;
                        }

                        _entries[i].hashCode = -1;
                        _entries[i].next = _freeList;
                        _entries[i].key = default(TKey);
                        _entries[i].value = default(TValue);
                        _freeList = i;
                        _freeCount++;
                        _version++;
                        return true;
                    }
                }
            }

            return false;
        }
        
        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_buckets == null)
            {
                Initialize(0);
            }

            // When you use % on negative value, you get a negative value. There are no negative buckets
            // so to avoid this you can remove the sign bit(the highest bit) and one way of doing this is to use a mask
            //  e.g.x & 0x7FFFFFFF which keeps all the bits except the top one.Another way to do this is to shift
            // the output x >>> 1 however this is slower
            var absoluteHashCode = _equalityComparer.GetHashCode(key) & NonNegativeInt32Mask;
            var bucketIndex = absoluteHashCode % _buckets.Length;

            int collisionCount = 0;

            for (int i = _buckets[bucketIndex]; i >= 0; i = _entries[i].next)
            {
                if (_entries[i].hashCode == absoluteHashCode && _equalityComparer.Equals(_entries[i].key, key))
                {
                    // This is the place where duplicate key exeption occures when you try Add key that already exist
                    if (add)
                    {
                        throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
                    }

                    _entries[i].value = value;
                    _version++;
                    return;
                }

                collisionCount++;
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = _entries[index].next;
                _freeCount--;
            }
            else
            {
                if (_count == _entries.Length)
                {
                    Resize();
                    bucketIndex = absoluteHashCode % _buckets.Length;
                }

                index = _count;
                _count++;
            }

            _entries[index].hashCode = absoluteHashCode;
            _entries[index].next = _buckets[bucketIndex];
            _entries[index].key = key;
            _entries[index].value = value;
            _buckets[bucketIndex] = index;
            _version++;

            // In case we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // in this case will be EqualityComparer<string>.Default.
            // Note, randomized string hashing is turned on by default on coreclr so EqualityComparer<string>.Default will 
            // be using randomized string hashing
 /*
            if (collisionCount > HashHelpers.HashCollisionThreshold && comparer == NonRandomizedStringEqualityComparer.Default) 
            {
                comparer = (IEqualityComparer<TKey>) EqualityComparer<string>.Default;
                Resize(entries.Length, true);
            }

            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(comparer)) 
            {
                comparer = (IEqualityComparer<TKey>) HashHelpers.GetRandomizedEqualityComparer(comparer);
                Resize(entries.Length, true);
            }
 */
        }

        private void Initialize(int capacity)
        {
            var size = CollectionHashtableHelpers.GetClosestLargerPrime(capacity);
            _buckets = new int[size];
            _entries = new Entry<TKey, TValue>[size];

            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = -1;
            }

            _freeList = -1;
        }

        private void Resize()
        {
            Resize(CollectionHashtableHelpers.ExpandPrime(_count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            Contract.Assert(newSize >= _entries.Length);
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            var newEntries = new Entry<TKey, TValue>[newSize];
            Array.Copy(_entries, 0, newEntries, 0, _count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (newEntries[i].hashCode != -1)
                    {
                        newEntries[i].hashCode = (_equalityComparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
                    }
                }
            }

            for (int i = 0; i < _count; i++)
            {
                if (newEntries[i].hashCode >= 0)
                {
                    int bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }

            _buckets = newBuckets;
            _entries = newEntries;
        }
    }
}
