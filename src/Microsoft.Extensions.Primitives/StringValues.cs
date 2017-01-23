// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.Primitives
{
    /// <summary>
    /// Represents zero/null, one, or many strings in an efficient way.
    /// </summary>
    public struct StringValues : IList<string>, IReadOnlyList<string>, IEquatable<StringValues>, IEquatable<string>, IEquatable<string[]>
    {
        private static readonly string[] EmptyArray = new string[0];
        public static readonly StringValues Empty = new StringValues(EmptyArray);

        private readonly object _value;

        public StringValues(string value)
        {
            _value = value;
        }

        public StringValues(string[] values)
        {
            _value = values;
        }

        public static implicit operator StringValues(string value)
        {
            return new StringValues(value);
        }

        public static implicit operator StringValues(string[] values)
        {
            return new StringValues(values);
        }

        public static implicit operator string (StringValues values)
        {
            return values.GetStringValue();
        }

        public static implicit operator string[] (StringValues value)
        {
            return value.GetArrayValue();
        }

        public int Count => (_value is string) ? 1 : GetArrayCount();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int GetArrayCount() => ((_value as string[])?.Length ?? 0);
        
        bool ICollection<string>.IsReadOnly
        {
            get { return true; }
        }

        string IList<string>.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        public string this[int index]
        {
            get
            {
                if (index == 0)
                {
                    var stringVal = _value as string;
                    if (stringVal != null)
                    {
                        return stringVal;
                    }
                }
                
                return GetArrayItem(index);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetArrayItem(int index)
        {
            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                return arrayValue[index]; // may throw
            }
            
            return EmptyArray[0]; // throws
        }

        public override string ToString()
        {
            return GetStringValue() ?? string.Empty;
        }

        private string GetStringValue()
        {
            var stringVal = _value as string;
            if (stringVal != null)
            {
                return stringVal;
            }
            
            return GetArrayStringValue();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetArrayStringValue()
        {
            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                switch (arrayValue.Length)
                {
                    case 0: return null;
                    case 1: return arrayValue[0];
                    default: return string.Join(",", arrayValue);
                }
            }
            return null;
        }
        
        public string[] ToArray()
        {
            return GetArrayValue() ?? EmptyArray;
        }

        private string[] GetArrayValue()
        {
            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                return arrayValue;
            }
            
            return GetStringArrayValue();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string[] GetStringArrayValue()
        {
            var stringVal = _value as string;
            if (stringVal != null)
            {
                return new[] { stringVal };
            }
            return null;
        }

        int IList<string>.IndexOf(string item)
        {
            return IndexOf(item);
        }

        private int IndexOf(string item)
        {
            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                for (int i = 0; i < arrayValue.Length; i++)
                {
                    if (string.Equals(arrayValue[i], item, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
                return -1;
            }

            return IndexOfString(item);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int IndexOfString(string item)
        {
            var stringVal = _value as string;
            if (stringVal != null)
            {
                return string.Equals(stringVal, item, StringComparison.Ordinal) ? 0 : -1;
            }

            return -1;
        }

        bool ICollection<string>.Contains(string item)
        {
            return IndexOf(item) >= 0;
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        private void CopyTo(string[] array, int arrayIndex)
        {
            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                Array.Copy(arrayValue, 0, array, arrayIndex, arrayValue.Length);
                return;
            }

            var stringVal = _value as string;
            if (stringVal != null)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }
                if (array.Length - arrayIndex < 1)
                {
                    throw new ArgumentException(
                        $"'{nameof(array)}' is not long enough to copy all the items in the collection. Check '{nameof(arrayIndex)}' and '{nameof(array)}' length.");
                }

                array[arrayIndex] = stringVal;
            }
        }

        void ICollection<string>.Add(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.Insert(int index, string item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<string>.Remove(string item)
        {
            throw new NotSupportedException();
        }

        void IList<string>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<string>.Clear()
        {
            throw new NotSupportedException();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool IsNullOrEmpty(StringValues value)
        {
            var stringVal = value._value as string;
            if (stringVal != null)
            {
                return string.IsNullOrEmpty(stringVal);
            }
            
            return value.IsNullOrEmptyArray();
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool IsNullOrEmptyArray()
        {
            var arrayValue = _value as string[];
            switch (arrayValue.Length)
            {
                case 0: return true;
                case 1: return string.IsNullOrEmpty(arrayValue[0]);
                default: return false;
            }
        }
        
        public static StringValues Concat(StringValues values1, StringValues values2)
        {
            var count1 = values1.Count;
            var count2 = values2.Count;

            if (count1 == 0)
            {
                return values2;
            }

            if (count2 == 0)
            {
                return values1;
            }

            var combined = new string[count1 + count2];
            values1.CopyTo(combined, 0);
            values2.CopyTo(combined, count1);
            return new StringValues(combined);
        }

        public static bool Equals(StringValues left, StringValues right)
        {
            var count = left.Count;

            if (count != right.Count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(StringValues left, StringValues right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringValues left, StringValues right)
        {
            return !Equals(left, right);
        }

        public bool Equals(StringValues other)
        {
            return Equals(this, other);
        }

        public static bool Equals(string left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool Equals(StringValues left, string right)
        {
            return Equals(left, new StringValues(right));
        }

        public bool Equals(string other)
        {
            return Equals(this, new StringValues(other));
        }

        public static bool Equals(string[] left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool Equals(StringValues left, string[] right)
        {
            return Equals(left, new StringValues(right));
        }

        public bool Equals(string[] other)
        {
            return Equals(this, new StringValues(other));
        }

        public static bool operator ==(StringValues left, string right)
        {
            return Equals(left, new StringValues(right));
        }

        public static bool operator !=(StringValues left, string right)
        {
            return !Equals(left, new StringValues(right));
        }

        public static bool operator ==(string left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool operator !=(string left, StringValues right)
        {
            return !Equals(new StringValues(left), right);
        }

        public static bool operator ==(StringValues left, string[] right)
        {
            return Equals(left, new StringValues(right));
        }

        public static bool operator !=(StringValues left, string[] right)
        {
            return !Equals(left, new StringValues(right));
        }

        public static bool operator ==(string[] left, StringValues right)
        {
            return Equals(new StringValues(left), right);
        }

        public static bool operator !=(string[] left, StringValues right)
        {
            return !Equals(new StringValues(left), right);
        }

        public static bool operator ==(StringValues left, object right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringValues left, object right)
        {
            return !left.Equals(right);
        }
        public static bool operator ==(object left, StringValues right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(object left, StringValues right)
        {
            return !right.Equals(left);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Equals(this, StringValues.Empty);
            }

            if (obj is string)
            {
                return Equals(this, (string)obj);
            }
            
            if (obj is string[])
            {
                return Equals(this, (string[])obj);
            }

            if (obj is StringValues)
            {
                return Equals(this, (StringValues)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            var stringVal = _value as string;
            if (stringVal != null)
            {
                return stringVal.GetHashCode();
            }

            var arrayValue = _value as string[];
            if (arrayValue != null)
            {
                var hcc = new HashCodeCombiner();
                for (var i = 0; i < arrayValue.Length; i++)
                {
                    hcc.Add(arrayValue[i]);
                }
                return hcc.CombinedHash;
            }

            return 0;
        }

        public struct Enumerator : IEnumerator<string>
        {
            private readonly string[] _values;
            private string _current;
            private int _index;

            public Enumerator(ref StringValues values)
            {
                _current = values._value as string;
                _values = values._value as string[];
                _index = 0;
            }

            public bool MoveNext()
            {
                if (_index < 0)
                {
                    return false;
                }

                if (_values != null)
                {
                    if (_index < _values.Length)
                    {
                        _current = _values[_index];
                        _index++;
                        return true;
                    }

                    _index = -1;
                    return false;
                }

                _index = -1; // sentinel value
                return _current != null;
            }

            public string Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }
    }
}
