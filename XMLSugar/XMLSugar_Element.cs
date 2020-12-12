using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLSugar
{
    public class XMLSugar_Element
    {
        public string Name;
        public string Value = null;

        public List<XMLSugar_ElementAttribute> Attributes = new List<XMLSugar_ElementAttribute>();

        public XMLSugar_Element Parent = null;
        public List<XMLSugar_Element> Childrens = new List<XMLSugar_Element>();

        public XMLSugar_Element(string name)
        {
            this.Name = name;
        }

        public XMLSugar_Element(XMLSugar_Element src)
        {
            this.Name = src.Name;
            this.Value = src.Value;

            this.Parent = src.Parent;
            foreach (var item in src.Attributes)
                this.Attributes.Add(new XMLSugar_ElementAttribute()
                {
                    Name = item.Name,
                    Value = item.Value
                });

            foreach (var item in src.Childrens)
                this.Childrens.Add(new XMLSugar_Element(item));
        }

        public XMLSugar_Element SetName(string name) { this.Name = name; return this; }
        public string GetName(string name) => this.Name;

        public XMLSugar_Element SetValue(object value) => this.SetValue(value.ToString());
        public XMLSugar_Element SetValue(string value) { this.Value = value; return this; }
        public string GetValue() => this.Value;

        public XMLSugar_Element SetAttributeValue(string attrName, object value) => this.SetAttributeValue(attrName, value.ToString());
        public XMLSugar_Element SetAttributeValue(string attrName, string value)
        {
            var foundAttr = this.Attributes.Where(t => t.Name.ToLower() == attrName.ToLower());
            if (foundAttr.Count() == 0) throw new Exception($"{attrName} attribute not found in element {this.Name}");

            foundAttr.First().Value = value;

            return this;
        }

        public string Print(int level = 0)
        {
            var prefix = new String(' ', level);
            var ret = $"{prefix}{this.Name} : {this.Value} ({this.Attributes.Count })\n";
            foreach (var item in this.Childrens)
                ret += item.Print(level + 1);
            return ret;
        }

        public bool Match(string selector)
        {
            var ret = true;

            var idx_Start = selector.IndexOf("#");

            var query_Start = selector.IndexOf("[");
            var query_End = selector.IndexOf("]");

            var name_End = selector.Length;
            if (idx_Start > -1)
                name_End = idx_Start;
            if (query_Start > -1)
                name_End = query_Start;

            var name = selector.Substring(0, name_End);

            var queriesDictionary = new Dictionary<string, string>();
            if (query_Start > -1)
            {
                var query = selector.Substring(query_Start + 1, query_End - query_Start - 1);
                var queries = query.Split(',');
                foreach (var item in queries)
                {
                    var query_parts = item.Split('=');
                    queriesDictionary[query_parts[0]] = query_parts[1];
                }
            }

            var idx = 0;
            if (idx_Start > -1)
                idx = int.Parse(selector.Substring(idx_Start + 1));

            if (this.Name.ToLower() == name.ToLower())
            {
                foreach (var item in queriesDictionary)
                {
                    if (this.Attributes.Where(q => (q.Name == item.Key) && (q.Value == item.Value)).Count() == 0)
                    {
                        ret = false;
                        break;
                    }
                }
            }
            else
                ret = false;

            return ret;
        }

        public XMLSugar_Element Access(string path, bool throwNotFound = false)
        {
            XMLSugar_Element ret = this;

            var path_parts = path.Split('/');
            foreach (var path_item in path_parts)
            {
                ret = ret.FindFirstOrNull(path_item);
                if (ret == null)
                {
                    if (!throwNotFound)
                        return null;
                    throw new KeyNotFoundException();
                }
            }

            return ret;
        }

        public List<XMLSugar_Element> AccessAll(string path, bool throwNotFound = false)
        {
            XMLSugar_Element self = this;

            var path_parts = path.Split('/');
            foreach (var path_item in path_parts)
            {
                self = self.FindFirstOrNull(path_item);
                if (self == null)
                {
                    if (!throwNotFound)
                        return new List<XMLSugar_Element>();
                    throw new KeyNotFoundException();
                }
            }

            return self.Parent.Find(path_parts.Last());
        }

        public List<XMLSugar_Element> Find(string selector, bool deep = true)
        {
            var ret = new List<XMLSugar_Element>();

            // Deep search in childrens
            if (deep)
            {
                if (this.Match(selector))
                    ret.Add(this);

                foreach (var item in this.Childrens)
                    ret.AddRange(item.Find(selector, deep));
            } else
            {
                foreach (var item in this.Childrens)
                    if (item.Match(selector))
                        ret.Add(item);
            }

            return ret;
        }

        public XMLSugar_Element FindFirstOrNull(string selector, bool deep = false, bool throwNotFound = false)
        {
            var results = this.Find(selector, deep);
            if (results.Count() == 0)
            {
                if (!throwNotFound)
                    return null;
                throw new KeyNotFoundException();
            }

            return results.First();
        }

        public void Clear()
        {
            this.Childrens.Clear();
        }

        public void Remove()
        {
            int idx = this.Parent.Childrens.IndexOf(this);
            if (idx == -1) throw new Exception($"Item {this.Name} not found in parent {this.Parent.Name} ");

            this.Parent.Childrens.RemoveAt(idx);
        }

        public void Replace(XMLSugar_Element newElement)
        {
            int idx = this.Parent.Childrens.IndexOf(this);
            if (idx == -1) throw new Exception($"Item {this.Name} not found in parent {this.Parent.Name} ");

            this.Parent.Childrens[idx] = newElement;
        }

        public void InsertBefore(XMLSugar_Element newElement)
        {
            int idx = this.Parent.Childrens.IndexOf(this);
            if (idx == -1) throw new Exception($"Item {this.Name} not found in parent {this.Parent.Name} ");

            newElement.Parent = this.Parent;
            this.Parent.Childrens.Insert(idx + 1, newElement);
        }

        public void InsertAfter(XMLSugar_Element newElement)
        {
            int idx = this.Parent.Childrens.IndexOf(this);
            if (idx == -1) throw new Exception($"Item {this.Name} not found in parent {this.Parent.Name} ");

            newElement.Parent = this.Parent;
            this.Parent.Childrens.Insert(idx + 1, newElement);
        }

        public void InsertInside(XMLSugar_Element newElement)
        {
            this.Value = null;

            newElement.Parent = this;
            this.Childrens.Add(newElement);
        }
    }
}
