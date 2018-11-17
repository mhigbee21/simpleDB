using System;
using System.Collections.Generic;

namespace SimpleDB
{
    //make a more friendly Dictionary...
    public class MyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        TValue _default;
        public TValue DefaultValue
        {
            get { return _default; }
            set { _default = value; }
        }
        public MyDictionary() : base() { }
        public MyDictionary(TValue defaultValue) : base()
        {
            _default = defaultValue;
        }
        public new TValue this[TKey key]
        {
            get
            {
                TValue t;
                return base.TryGetValue(key, out t) ? t : _default;
            }
            set { base[key] = value; }
        }


        public TValue Get(TKey key)
        {
            TValue t;
            if (base.TryGetValue(key, out t))
            {
                return t;
            }
            return t;
        }

        public string GetAsString(TKey key)
        {
            TValue t;
            if (base.TryGetValue(key, out t))
            {
                return t as string;
            }
            return string.Empty;
        }

        public int GetAsInt(TKey key)
        {
            TValue t;
            if (base.TryGetValue(key, out t))
            {
                return int.Parse(t as string);
            }
            return 0;
        }

        public Guid GetAsGuid(TKey key)
        {
            TValue t;
            if (base.TryGetValue(key, out t))
            {
                return Guid.Parse(t as string);
            }
            return Guid.Parse(string.Empty);
        }

        public Boolean GetAsBool(TKey key)
        {
            TValue t;
            if (base.TryGetValue(key, out t))
            {
                string result = t as string;
                if (result != null)
                {
                    if (result.ToUpper() == "TRUE")
                    {
                        return true;
                    }
                    if (result == string.Empty)
                    {
                        return false;
                    }
                    if (result.Length > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public void Add(TKey key, TValue val)
        {
            if (base.ContainsKey(key))
            {
                base[key] = val;
            }
            else
            {
                base.Add(key, val);
            }
        }
    }
}
