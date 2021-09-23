using System;
using System.Collections.Generic;
using System.Text;

namespace XMLSugar
{
    public abstract class XMLSugar_Instance
    {
        public XMLSugar_Element _element = null;

        public abstract XMLSugar_Element Example();

        public abstract bool FromElement(XMLSugar_Element element);
        public abstract void ToElement(XMLSugar_Element element);

        public void ToFile(string path) => this._element.ToFile(path);

        public string ToXML()
        {
            this._element = this._element ?? this.Example();
            return this._element.ToXML();
        }

        public static T FromExample<T>() where T : XMLSugar_Instance, new()
        {
            var ret = new T();
            var elem = (ret.Example());
            return elem.Materialize<T>();
        }
    }
}
