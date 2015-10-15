// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Extensions.Internal
{
    internal class CopyOnWriteDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private CopyOnWriteDictionaryHolder<IDictionary<TKey, TValue>, TKey, TValue> _holder;

        public CopyOnWriteDictionary(
            IDictionary<TKey, TValue> source,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            _holder = new CopyOnWriteDictionaryHolder<IDictionary<TKey, TValue>, TKey, TValue>(
                source,
                (d) => new Dictionary<TKey, TValue>(d, comparer));
        }

        public CopyOnWriteDictionary(
            IDictionary<TKey, TValue> source,
            Func<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>> factory)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _holder = new CopyOnWriteDictionaryHolder<IDictionary<TKey, TValue>, TKey, TValue>(
                source,
                factory);
        }

        public virtual ICollection<TKey> Keys => _holder.Keys;

        public virtual ICollection<TValue> Values => _holder.Values;

        public virtual int Count => _holder.Count;

        public virtual bool IsReadOnly => _holder.IsReadOnly;

        public virtual TValue this[TKey key]
        {
            get
            {
                return _holder[key];
            }
            set
            {
                _holder[key] = value;
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            return _holder.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            _holder.Add(key, value);
        }

        public virtual bool Remove(TKey key)
        {
            return _holder.Remove(key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return _holder.TryGetValue(key, out value);
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            _holder.Add(item);
        }

        public virtual void Clear()
        {
            _holder.Clear();
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _holder.Contains(item);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _holder.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _holder.Remove(item);
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _holder.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}