// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Extensions.Internal
{
    internal struct CopyOnWriteDictionaryHolder<TDictionary, TKey, TValue>
        where TDictionary : class, IDictionary<TKey, TValue>
    {
        private readonly TDictionary _source;
        private readonly Func<TDictionary, TDictionary> _factory;
        private TDictionary _copy;

        public CopyOnWriteDictionaryHolder(TDictionary source, Func<TDictionary, TDictionary> factory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _source = source;
            _factory = factory;

            _copy = null;
        }

        public CopyOnWriteDictionaryHolder(CopyOnWriteDictionaryHolder<TDictionary, TKey, TValue> source)
        {
            _source = source.HasBeenCopied ? source.WriteDictionary : source.ReadDictionary;
            _factory = source._factory;
            _copy = null;
        }

        public bool HasBeenCopied => _copy != null;

        public TDictionary ReadDictionary
        {
            get
            {
                return _copy ?? _source;
            }
        }

        public TDictionary WriteDictionary
        {
            get
            {
                if (_copy == null)
                {
                    _copy = _factory(_source);
                }

                return _copy;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return ReadDictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return ReadDictionary.Values;
            }
        }

        public int Count
        {
            get
            {
                return ReadDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return ReadDictionary[key];
            }
            set
            {
                WriteDictionary[key] = value;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return ReadDictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            WriteDictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return WriteDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ReadDictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            WriteDictionary.Add(item);
        }

        public void Clear()
        {
            WriteDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ReadDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ReadDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return WriteDictionary.Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ReadDictionary.GetEnumerator();
        }
    }
}
