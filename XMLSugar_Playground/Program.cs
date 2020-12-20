using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XMLSugar;

namespace XMLSugar_Playground
{
    class Program
    {
        public class TIAXML_WatchTable_Item : XMLSugar_Instance
        {
            public int ID;
            public string name;
            public string datatype;
            public string address;

            public override XMLSugar_Element Example() => new XMLSugar.XMLSugar(@"
      <SW.WatchAndForceTables.PlcWatchTableEntry ID=""0"" CompositionName=""Entries"">
        <AttributeList>
          <Address ReadOnly=""true"">ADDRESS</Address>
          <DisplayFormat>DATATYPE</DisplayFormat>
          <ModifyIntention ReadOnly=""true"">false</ModifyIntention>
          <ModifyTrigger>Permanent</ModifyTrigger>
          <ModifyValue />
          <MonitorTrigger>Permanent</MonitorTrigger>
          <Name>""NAME""</Name>
        </AttributeList>
      </SW.WatchAndForceTables.PlcWatchTableEntry>
").rootElement;

            public override void FromElement(XMLSugar_Element element)
            {
                this.ID = int.Parse(element.GetAttributeValue("ID"), System.Globalization.NumberStyles.HexNumber);

                this.name = element.Access("AttributeList/Name").GetValue() ?? "";
                this.datatype = element.Access("AttributeList/DisplayFormat").GetValue();
                this.address = element.Access("AttributeList/Address").GetValue();
            }

            public override void ToElement(XMLSugar_Element element)
            {
                element.SetAttributeValue("ID", this.ID.ToString("X"));

                element.Access("AttributeList/Name").SetValue(this.name);
                element.Access("AttributeList/DisplayFormat").SetValue(this.datatype);
                element.Access("AttributeList/Address").SetValue(this.address);
            }
        }

        
        public class TIAXML_DBMember_Item : XMLSugar_Instance
        {
            public string name;
            public string datatype;

            public IList<TIAXML_DBMember_Item> Childrens;

            public TIAXML_DBMember_Item() { }

            public TIAXML_DBMember_Item(TIAXML_DBMember_Item src)
            {
                this.name = src.name;
                this.datatype = src.datatype;

                this.Childrens = new List<TIAXML_DBMember_Item>();
                foreach (var item in src.Childrens)
                    this.Childrens.Add(new TIAXML_DBMember_Item(item));
            }

            public override XMLSugar_Element Example() => new XMLSugar.XMLSugar(@"
    <Member Name=""Home"" Datatype=""Bool"">
    </Member>
").rootElement;

            public override void FromElement(XMLSugar_Element element)
            {
                this.Childrens = element.AccessCollection<TIAXML_DBMember_Item>("", "Member");

                this.name = element.GetAttributeValue("Name");
                this.datatype = element.GetAttributeValue("Datatype");
            }

            public override void ToElement(XMLSugar_Element element)
            {
                element.SetAttributeValue("Name", this.name);
                element.SetAttributeValue("Datatype", this.datatype);
            }
        }


        public class DeveloperScope
        {
            private Dictionary<string, object> scope = new Dictionary<string, object>();
            public object this[string key]
            {
                get => this.scope[key];
                set { this.scope[key] = value; }
            }
        }


        static void Main(string[] args)
        {
            /*
            var devWatchTable = new WatchTable_Developer<TIAXML_WatchTable_Worker>();

            devWatchTable.RemoveSymbol("\"SQ84/BA\"");

            devWatchTable.AddSymbol("HelloWorld", "TEST", "M776.0");

            devWatchTable.Generate();
            */
            /*
            var scope = new DeveloperScope();
            scope["abc"] = "abc";
            */

            var xml = new XMLSugar.XMLSugar();
            xml.LoadFromFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\IO_ST_A.xml");

            var inst1 = xml.rootElement.Materialize<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry[ID=8]");

            var inst3 = xml.rootElement.Materialize<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry[ID=F]");
            inst3.name = "Hello world";

            var test = xml.rootElement.FindFirstOrNull("Name[Value=\"SQ57/BA\"]").Materialize<TIAXML_WatchTable_Item>();

            var childrens = xml.rootElement.AccessCollection<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList", "SW.WatchAndForceTables.PlcWatchTableEntry");


            childrens.Remove(inst1);

            var childrens2 = xml.rootElement.AccessCollection<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList", "SW.WatchAndForceTables.PlcWatchTableEntry");

            childrens2.Remove(inst3);

            var aa = childrens[0].name;

            xml.SaveToFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\test.xml");
            /*
            var xml2 = XMLSugar.XMLSugar.FromFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\PROAUTOMATOR_GLOBAL.xml");

            var coll10 = xml2.rootElement.AccessCollection<TIAXML_DBMember_Item>("SW.Blocks.GlobalDB/AttributeList/Interface/Sections/Section[Name=Static]", "Member");

            var actuators = coll10[0];
            var ev = actuators.Childrens[0];
            var ls = ev.Childrens[0];

            ls.Childrens.Add(new TIAXML_DBMember_Item(ls.Childrens[0]));

            ls.Childrens[0].name = "Pippo";


            xml2.SaveToFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\test2.xml");
            */
            Console.WriteLine("Done.");
            Console.WriteLine(Directory.GetCurrentDirectory());

            Console.ReadLine();
            Console.WriteLine("Hello World!");
        }
    }
}
