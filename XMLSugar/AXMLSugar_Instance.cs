using System;
using System.Collections.Generic;
using System.Text;

namespace XMLSugar
{
    public abstract class AXMLSugar_Instance
    {
        private XMLSugar_Element _element = null;

        public abstract XMLSugar_Element Example();

        public abstract void FromElement(XMLSugar_Element element);
        public abstract void ToElement(XMLSugar_Element element);
    }
}
