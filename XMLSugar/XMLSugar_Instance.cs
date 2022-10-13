using System;
using System.Collections.Generic;
using System.Text;

namespace XMLSugar
{
    public abstract class XMLSugar_Instance
    {
        public XMLSugar_Element _element = null;

        public virtual XMLSugar_Instance Create(XMLSugar_Element element)
        {
            return null;
        }

        public abstract XMLSugar_Element Example();

        public abstract bool FromElement(XMLSugar_Element element);
        public abstract void ToElement(XMLSugar_Element element);

        public void ToFile(string path) => this._element.ToFile(path);

        public XMLSugar_Element GetElement()
        {
            this._element = this._element ?? this.Example();
            this.ToElement(this._element);
            return this._element;
        }

        public string ToXML()
        {
            return this.GetElement().ToXML();
        }

        public static T FromExample<T>() where T : XMLSugar_Instance, new()
        {
            var ret = new T();
            var elem = (ret.Example());
            return elem.Materialize<T>();
        }

        public T Clone<T>() where T : XMLSugar_Instance, new()
        {
            return XMLSugar_Element.FromXML(this.ToXML()).Materialize<T>();
        }
    }
}
