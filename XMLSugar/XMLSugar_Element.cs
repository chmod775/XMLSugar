using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XMLSugar
{
    public class XMLSugar_Element
    {
        public string Name;
        public string Value = null;

        internal XLMSugar_Link _link = new XLMSugar_Link();

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

        #region File
        private static Dictionary<string, XMLSugar_Element> cache = new Dictionary<string, XMLSugar_Element>();

        public static XMLSugar_Element FromFile(string path, bool ignoreCache = false)
        {
            if (!ignoreCache)
                if (cache.ContainsKey(path))
                    return cache[path];

            XMLSugar_Element ret = null;
            if (!File.Exists(path)) return null;

            using (StreamReader r = new StreamReader(path))
            {
                string xmlraw = r.ReadToEnd();
                ret = XMLSugar_Element.FromXML(xmlraw);
            }

            if (ret == null) throw new Exception($"Cannot load file {path}");

            cache[path] = ret;
            return ret;
        }

        public void ToFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter w = new StreamWriter(path, false))
            {
                w.Write(this.ToXML());
            }
        }
        #endregion

        #region XML
        public static XMLSugar_Element FromXML(string xml)
        {
            using (var sr = new StringReader(xml))
            {
                var _xmlReaderSettings = new XmlReaderSettings();
                _xmlReaderSettings.IgnoreComments = true;
                _xmlReaderSettings.IgnoreWhitespace = true;
                _xmlReaderSettings.IgnoreProcessingInstructions = true;

                var _xmlReader = XmlReader.Create(sr, _xmlReaderSettings);

                _xmlReader.Read();
                if (_xmlReader.NodeType == XmlNodeType.XmlDeclaration)
                    XMLSugar_Element.ParseNext(_xmlReader, XmlNodeType.XmlDeclaration);

                return XMLSugar_Element.ParseReader(_xmlReader);
            }
        }

        public string ToXML()
        {
            using (var sw = new StringWriterUTF8())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;

                using (var writer = XmlWriter.Create(sw, settings))
                {
                    this.GenerateWriter(writer);
                }
                return sw.ToString();
            }
        }
        #endregion


        #region Parser
        private static bool ParseNext(XmlReader reader, XmlNodeType expectedType)
        {
            var ret = true;

            if (reader.NodeType != XmlNodeType.None)
            {
                if (reader.NodeType != expectedType)
                    ret = false;
                reader.Read();

                if (!ret)
                    throw new Exception("Expected node not matching");
            }

            return ret;
        }

        private static XMLSugar_Element ParseReader(XmlReader reader, XMLSugar_Element parentElement = null)
        {
            XMLSugar_Element ret = new XMLSugar_Element(reader.Name);

            ret.Value = null;

            ret.Parent = parentElement;

            var isEmpty = reader.IsEmptyElement;

            // Read attributes
            if (reader.AttributeCount > 0)
            {
                while (reader.MoveToNextAttribute())
                {
                    var newAttr = new XMLSugar_ElementAttribute()
                    {
                        Name = reader.Name,
                        Value = reader.Value
                    };
                    ret.Attributes.Add(newAttr);
                }
                reader.MoveToElement();
            }

            XMLSugar_Element.ParseNext(reader, XmlNodeType.Element);

            if (isEmpty)
                return ret;

            while (reader.NodeType == XmlNodeType.Element)
            {
                var childrenElement = XMLSugar_Element.ParseReader(reader, ret);
                if (childrenElement == null) break;
                ret.Childrens.Add(childrenElement);
            }

            if (reader.NodeType == XmlNodeType.Text)
            {
                ret.Value = reader.Value;
                XMLSugar_Element.ParseNext(reader, XmlNodeType.Text);
            }

            XMLSugar_Element.ParseNext(reader, XmlNodeType.EndElement);

            return ret;
        }
        #endregion



        #region Setters and Getters
        public XMLSugar_Element SetName(string name) { this.Name = name; return this; }
        public string GetName(string name) => this.Name;

        public XMLSugar_Element SetValue(object value) => this.SetValue(value.ToString());
        public XMLSugar_Element SetValue(string value) { this.Value = value; return this; }
        public string GetValue() => this.Value;

        public XMLSugar_Element SetAttributeValue(string attrName, object value) => this.SetAttributeValue(attrName, value.ToString());
        public XMLSugar_Element SetAttributeValue(string attrName, string value)
        {
            var foundAttr = this.Attributes.Where(t => t.Name.ToLower() == attrName.ToLower());
            if (foundAttr.Count() == 0)
            {
                this.Attributes.Add(new XMLSugar_ElementAttribute()
                {
                    Name = attrName,
                    Value = value
                });
                return this;
            }

            foundAttr.First().Value = value;
            return this;
        }
        public string GetAttributeValue(string attrName)
        {
            var foundAttr = this.Attributes.Where(t => t.Name.ToLower() == attrName.ToLower());
            if (foundAttr.Count() == 0) throw new Exception($"{attrName} attribute not found in element {this.Name}");
            return foundAttr.First().Value;
        }
        public bool HasAttribute(string attrName)
        {
            var foundAttr = this.Attributes.Where(t => t.Name.ToLower() == attrName.ToLower());
            return foundAttr.Count() > 0;
        }
        public void RemoveAttribute(string attrName) => this.Attributes.RemoveAll(t => t.Name.ToLower() == attrName.ToLower());
        #endregion

        public string Print(int level = 0)
        {
            var prefix = new String(' ', level);
            var ret = $"{prefix}{this.Name} : {this.Value} ({this.Attributes.Count })\n";
            foreach (var item in this.Childrens)
                ret += item.Print(level + 1);
            return ret;
        }

        private bool Match(string selector)
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
                    if (item.Key.ToLower() == "value")
                    {
                        if (this.Value != item.Value)
                            ret = false;
                    } else
                    {
                        if (this.Attributes.Where(q => (q.Name == item.Key) && (q.Value == item.Value)).Count() == 0)
                        {
                            ret = false;
                            break;
                        }
                    }
                }
            }
            else
                ret = false;

            return ret;
        }

        #region Access
        public List<XMLSugar_Element> Access(string path)
        {
            XMLSugar_Element self = this;
            if (path.Length == 0) return new List<XMLSugar_Element>() { self };

            var path_parts = path.Split('/');

            foreach (var path_item in path_parts)
            {
                self = self.FindFirstOrNull(path_item);
                if (self == null)
                    return new List<XMLSugar_Element>();
            }

            return self.Parent.Find(path_parts.Last());
        }

        public XMLSugar_Element AccessFirstOrNull(string path)
        {
            var results = this.Access(path);
            if (results.Count() == 0)
                return null;
            return results.First();
        }
        #endregion

        #region Find
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
            }
            else
            {
                foreach (var item in this.Childrens)
                    if (item.Match(selector))
                        ret.Add(item);
            }

            return ret;
        }
        public XMLSugar_Element FindFirstOrNull(string selector, bool deep = false)
        {
            var results = this.Find(selector, deep);
            if (results.Count() == 0)
                return null;
            return results.First();
        }

        public XMLSugar_Element FindParent(string selector)
        {
            var ret = this.Parent;
            while (ret != null)
            {
                if (ret.Match(selector))
                    return ret;
                ret = ret.Parent;
            }

            return null;
        }
        #endregion


        #region Materialize
        public static T MaterializeFile<T>(string path, bool ignoreCache = false) where T : XMLSugar_Instance, new() =>  XMLSugar_Element.FromFile(path, ignoreCache).Materialize<T>();

        public static bool Materialize<T>(T dest) where T : XMLSugar_Instance, new()
        {
            var foundElement = dest._element ?? (dest.Example());
            if (foundElement == null) throw new Exception("Element not defined");

            dest._element = foundElement;

            var validElement = dest.FromElement(dest._element);
            if (!validElement) return false;

            foundElement._link.Single = dest;
            return true;
        }

        public T Materialize<T>(string path = "") where T : XMLSugar_Instance, new()
        {
            T ret = new T();

            var foundElement = this.AccessFirstOrNull(path) ?? (ret.Example());
            if (foundElement._link.Collection != null) throw new Exception("Element is already linked to a collection. Access using AccessCollection.");
            if (foundElement._link.Single != null) return (T)foundElement._link.Single;

            ret._element = foundElement;

            if (!Materialize<T>(ret)) return null;
            return ret;
        }

        public IList<T> MaterializeCollection<T>(string path, string selector, bool immutable = false) where T : XMLSugar_Instance, new()
        {
            var ret = new List<T>();

            var collectionElement = this.AccessFirstOrNull(path);
            if (collectionElement == null) throw new Exception($"Collection element {path} not found.");
            if (collectionElement._link.Single != null) throw new Exception("Element is already linked to as single. Access using AccessSingle.");

            if (!immutable)
            {
                collectionElement._link.Collection = collectionElement._link.Collection ?? new Dictionary<Type, IList>();
                if (collectionElement._link.Collection.ContainsKey(typeof(T)))
                    return collectionElement._link.Collection[typeof(T)] as IList<T>;

                collectionElement._link.Collection[typeof(T)] = ret;
            }

            var foundElements = collectionElement.Find(selector, false);

            foreach (var item in foundElements)
            {
                T newItemIstance = item.Materialize<T>();
                if (newItemIstance != null)
                    ret.Add(newItemIstance);
            }

            return ret;
        }

        public IDictionary<string, string> MaterializeDictionary(string path, Func<string, bool> selector)
        {
            var ret = new Dictionary<string, string>();

            var collectionElement = this.AccessFirstOrNull(path);
            if (collectionElement == null) throw new Exception($"Collection element {path} not found.");
            if (collectionElement._link.Single != null) throw new Exception("Element is already linked to as single. Access using AccessSingle.");



            return ret;
        }

        #endregion

        #region Map
        public T Map<T>(Func<XMLSugar_Element, T> mapper) where T : XMLSugar_Instance, new()
        {
            var ret = mapper(this);
            this._link.Single = ret;
            return ret;
        }

        public IList<T> MapCollection<T>(string path, string selector, Func<XMLSugar_Element, T> mapper) where T : XMLSugar_Instance
        {
            var ret = new List<T>();

            var collectionElement = this.AccessFirstOrNull(path);
            if (collectionElement == null) throw new Exception($"Collection element {path} not found.");
            if (collectionElement._link.Single != null) throw new Exception("Element is already linked to as single. Access using AccessSingle.");

            collectionElement._link.Collection = collectionElement._link.Collection ?? new Dictionary<Type, IList>();
            if (collectionElement._link.Collection.ContainsKey(typeof(T)))
                return collectionElement._link.Collection[typeof(T)] as IList<T>;

            collectionElement._link.Collection[typeof(T)] = ret;

            var foundElements = collectionElement.Find(selector, false);

            foreach (var item in foundElements)
            {
                T newItemIstance = mapper(item);
                if (newItemIstance != null)
                    ret.Add(newItemIstance);
            }

            return ret;
        }
        #endregion

        #region Create
        public T CreateIstanceInside<T>() where T : XMLSugar_Instance, new()
        {
            T ret = new T();

            var newElement = (ret.Example());
            ret._element = newElement;
            ret.FromElement(newElement);

            newElement._link.Single = ret;

            this.InsertInside(newElement);

            return ret;
        }

        public static XMLSugar_Element CreateElement(XMLSugar_Instance instance)
        {
            var ret = instance._element ?? instance.Example();
            ret._link.Single = instance;
            instance.ToElement(ret);

            instance._element = ret;

            return ret;
        }

        public XMLSugar_Element CreateElementInside(XMLSugar_Instance instance)
        {
            var ret = XMLSugar_Element.CreateElement(instance);
            this.InsertInside(ret);
            return ret;
        }
        #endregion

        #region Editing
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
        #endregion

        internal void GenerateWriter(XmlWriter writer)
        {
            if (this._link.Single != null)
                this._link.Single.ToElement(this);

            // Genearete attributes
            var namespc = this.Attributes.Where(t => t.Name == "xmlns");
            if (namespc.Count() > 0)
                writer.WriteStartElement(this.Name, namespc.First().Value);
            else
                writer.WriteStartElement(this.Name);

            foreach (var attr in this.Attributes)
            {
                if (attr.Name != "xmlns")
                    writer.WriteAttributeString(attr.Name, attr.Value);
            }


            // Group all collection instances
            List<XMLSugar_Instance> collectionInstances = new List<XMLSugar_Instance>();
            if (this._link.Collection != null)
            {
                foreach (var typeItem in this._link.Collection)
                    foreach (var instanceItem in typeItem.Value)
                        collectionInstances.Add((XMLSugar_Instance)instanceItem);

                // Remove childrens not present in collection
                this.Childrens.RemoveAll(t =>
                {
                    if (t._link.Single == null) return false;
                    return !collectionInstances.Contains(t._link.Single);
                });

                /*
                foreach (var item in this.Childrens)
                {
                    if (item._link.Single == null) item.GenerateWriter(writer);
                }


                foreach (var typeItem in this._link.Collection)
                    foreach (var instanceItem in typeItem.Value)
                    {
                        var item = instanceItem as XMLSugar_Instance;
                        if (item._element != null)
                        {
                            item._element._link.Single.ToElement(item._element);
                            item._element.GenerateWriter(writer);
                        }
                        else
                        {
                            var newElementForIstance = this.CreateElementInside(item);
                            newElementForIstance.GenerateWriter(writer);
                        }
                    }
                */




                
                foreach (var item in this.Childrens)
                {

                    if (item._link.Single != null)
                    {
                        var foundCollectionIdx = collectionInstances.IndexOf(item._link.Single);
                        if (foundCollectionIdx > -1)
                        {
                            item.GenerateWriter(writer);
                            item._link.Single.ToElement(item);
                            collectionInstances[foundCollectionIdx] = null; // Flag instance as already processed
                        }
                        else
                            throw new TimeZoneNotFoundException();
                    } else
                    {
                        //item.GenerateWriter(writer);
                    }
                }

                // Add new instances present in collection to childrens
                var newCollectionInstances = collectionInstances.Where(t => t != null).ToList();
                foreach (var item in newCollectionInstances)
                {
                    var newElementForIstance = this.CreateElementInside(item);
                    newElementForIstance.GenerateWriter(writer);
                }
            }
            else
            {
                foreach (var item in this.Childrens)
                {
                    item.GenerateWriter(writer);
                }
            }


            if (this.Value != null)
                writer.WriteValue(this.Value);

            writer.WriteEndElement();
        }
    }
}
/*

                foreach (var item in this.Childrens)
                {

                    if (item._link.Single != null)
                    {
                        var foundCollectionIdx = collectionInstances.IndexOf(item._link.Single);
                        if (foundCollectionIdx > -1)
                        {
                            collectionInstances[foundCollectionIdx] = null; // Flag instance as already processed
                        }
                        else
                            throw new TimeZoneNotFoundException();
                    } else
                    {
                        //item.GenerateWriter(writer);
                    }
                }

                // Add new instances present in collection to childrens
                var newCollectionInstances = collectionInstances.Where(t => t != null).ToList();
                foreach (var item in newCollectionInstances)
                {
                    var typeElements = this.Childrens.Where(t => (t._link.Single == null) ? false : t._link.Single.GetType().Equals(item.GetType())).LastOrNull();
                    
                    var newElementForIstance = XMLSugar_Element.CreateElement(item);


                    if (typeElements != null)
                    {
                        typeElements.InsertAfter(newElementForIstance);
                    }
                    else
                    {
                        this.InsertInside(newElementForIstance);
                    }
                    //newElementForIstance.GenerateWriter(writer);

                }

                foreach (var item in this.Childrens)
                {
                    if (item._link.Single != null)
                    {
                        item.GenerateWriter(writer);
                        item._link.Single.ToElement(item);
                    }
                    else
                    {
                        item.GenerateWriter(writer);
                    }
                }
*/