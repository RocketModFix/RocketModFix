using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Rocket.API.Collections
{
    [Serializable]
    [XmlType(AnonymousType = false, IncludeInSchema = true, TypeName = "Translation")]


    public class TranslationListEntry
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Value;

        public TranslationListEntry(string id, string value)
        {
            Id = id;
            Value = value;
        }
        public TranslationListEntry() { }
    }







    public static class TranslationListExtension{
        public static void AddUnknownEntries(this TranslationList defaultTranslations, IAsset<TranslationList> translations)
        {
            bool hasChanged = false;
            foreach(TranslationListEntry entry in defaultTranslations)
            {
                if (translations.Instance[entry.Id] == null) {
                    translations.Instance.Add(entry);
                    hasChanged = true;
                }
            }
            if(hasChanged)
                translations.Save();
        }
    }

    [XmlRoot("Translations")]
    [XmlType(AnonymousType = false, IncludeInSchema = true, TypeName = "Translation")]
    [Serializable]


    public class TranslationList : IDefaultable, ICollection<TranslationListEntry>
    {
        public TranslationList() { foreach (TranslationListEntry entry in translations) _translations[entry.Id] = entry.Value; }

        protected List<TranslationListEntry> translations = new List<TranslationListEntry>();
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        public int Count => translations.Count;

        public bool IsReadOnly => false;

        public string Translate(string translationKey, params object[] placeholder)
        {
            string value = this[translationKey];
            if (String.IsNullOrEmpty(value)) return translationKey;

            if (value.Contains("{") && value.Contains("}") && placeholder != null && placeholder.Length != 0)
            {
                for (int i = 0; i < placeholder.Length; i++)
                {
                    if (placeholder[i] == null) placeholder[i] = "NULL";
                }
                value = String.Format(value, placeholder);
            }
            return value;
        }
        public void Add(Object o)
        {
            translations.Add((TranslationListEntry)o);
            TranslationListEntry entry = (TranslationListEntry)o;
            _translations[entry.Id] = entry.Value;
        }
        public void Add(string key, string value)
        {
            translations.Add(new TranslationListEntry(key, value));
            _translations[key] = value;
        }
        public void AddRange(IEnumerable<TranslationListEntry> collection)
        {
            translations.AddRange(collection);
            foreach (TranslationListEntry entry in collection) _translations[entry.Id] = entry.Value;
        }
        public void AddRange(TranslationList collection)
        {
            translations.AddRange(collection.translations);
            foreach (TranslationListEntry entry in collection.translations) _translations[entry.Id] = entry.Value;
        }

        public string this[string key]
        {
            get
            {
                _translations.TryGetValue(key, out string val);
                return val;
                //return translations.Where(k => k.Id == key).Select(k => k.Value).FirstOrDefault();
            }
            set
            {
                translations.ForEach(k => { if (k.Id == key) k.Value = value; });
                _translations[key] = value;
            }
        }
        public virtual void LoadDefaults()
        {

        }
        public IEnumerator<TranslationListEntry> GetEnumerator()
        {
            return translations.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return translations.GetEnumerator();
        }

        public void Add(TranslationListEntry item)
        {
            translations.Add(item);
            _translations[item.Id] = item.Value;
        }

        public void Clear()
        {
            translations.Clear();
            _translations.Clear();
        }

        public bool Contains(TranslationListEntry item)
        {
            return _translations.ContainsKey(item.Id);
        }

        public void CopyTo(TranslationListEntry[] array, int arrayIndex)
        {
            translations.CopyTo(array, arrayIndex);
        }

        public bool Remove(TranslationListEntry item)
        {
            _translations.Remove(item.Id);
            return translations.Remove(item);
        }
    }



}
