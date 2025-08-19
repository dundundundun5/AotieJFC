using System;
using System.Collections.Generic;

namespace AlgorithmAcceptanceTool.Models
{
    public class SafeDictionary<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        public TValue this[TKey key]
        {
            get
            {
                lock (_Padlock)
                    return _dictionary[key];
            }
            set
            {
                lock (_Padlock)
                    _dictionary[key] = value;
            }
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_Padlock)
                return _dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            lock (_Padlock)
                return _dictionary.ContainsKey(key);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            lock (_Padlock)
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];
                var value = valueFactory(key);
                _dictionary.Add(key, value);
                return value;
            }
        }


        public void Add(TKey key, TValue value)
        {
            lock (_Padlock)
                _dictionary.Add(key, value);
        }

        public void Clear()
        {
            lock (_Padlock)
                _dictionary.Clear();
        }

        public bool Any()
        {
            lock (_Padlock)
                return _dictionary.Count > 0;
        }

        public Dictionary<TKey, TValue> Inner
        {
            get
            {
                return _dictionary;
            }
        }
    }
}
