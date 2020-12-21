using System;
using System.Collections.Generic;
using System.Text;

namespace XMLSugar
{
    public abstract class XMLSugar_Instance
    {
        internal XMLSugar_Element _element = null;

        public abstract XMLSugar_Element Example();

        public abstract bool FromElement(XMLSugar_Element element);
        public abstract void ToElement(XMLSugar_Element element);
    }
}
