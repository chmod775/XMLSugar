using System;
using System.Collections.Generic;
using System.IO;
using XMLSugar;

namespace XMLSugar_Playground
{
    class Program
    {
        public class TIAXML_WatchTable_Item : XMLSugar_Instance
        {
            public string ID;
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
                this.ID = element.GetAttributeValue("ID");

                this.name = element.Access("AttributeList/Name").GetValue();
                this.datatype = element.Access("AttributeList/DisplayFormat").GetValue();
                this.address = element.Access("AttributeList/Address").GetValue();
            }

            public override void ToElement(XMLSugar_Element element)
            {
                element.SetAttributeValue("ID", this.ID);

                element.Access("AttributeList/Name").SetValue(this.name);
                element.Access("AttributeList/DisplayFormat").SetValue(this.datatype);
                element.Access("AttributeList/Address").SetValue(this.address);
            }
        }

        static void Main(string[] args)
        {
            var xml = new XMLSugar.XMLSugar();
            xml.LoadFromFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\IO_ST_A.xml");

            var inst1 = xml.rootElement.AccessSingle<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry[ID=8]");

            var inst3 = xml.rootElement.AccessSingle<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry[ID=F]");
            inst3.name = "Hello world";

            var coll1 = xml.rootElement.AccessCollection<TIAXML_WatchTable_Item>("SW.WatchAndForceTables.PlcWatchTable/ObjectList", "SW.WatchAndForceTables.PlcWatchTableEntry");

            coll1.Add(new TIAXML_WatchTable_Item()
            {
                name = "Prova",
                address = "M77.5",
                datatype = "Bool",
                ID = "775"
            });

            coll1.Remove(inst1);

            //var inst2 = elem1.CreateInside<TIAXML_WatchTable_Item>();
            //inst2.ID = 775;
            //inst2.name = "New";

            //var item1 = xml.Access("SW.WatchAndForceTables.PlcWatchTable/AttributeList/Name");
            //var items = xml.AccessAll("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry");

            /*
            //var item1 = xml.Access("SW.Blocks.GlobalDB/AttributeList/Interface/Sections/Section[Name=Static]");
            var item1 = xml.FindFirstOrNull("Section[Name=Static]", true);

            //var memberBool = item1.Access("Member[Name=ACTUATORS]/Member[Name=EV]/Member[Name=LimitSwitches]/Member[Name=Home]");
            var memberBool = item1.FindFirstOrNull("Member[Name=Home]", true);

            item1.Clear();

            var names = new List<string>() { "Y1", "Y2A", "Y2B", "Y3" };
            foreach (var item in names)
            {
                var newMember = new XMLSugar.XMLSugar_Element(memberBool);
                newMember.SetAttributeValue("Name", item);
                item1.InsertInside(newMember);
            }

            var xml_str = xml.ToXML();
            */
            xml.SaveToFile(@"C:\Users\miche\source\repos\XMLSugar\XMLSugar_Playground\Examples\test.xml");

            Console.WriteLine("Done.");
            Console.WriteLine(Directory.GetCurrentDirectory());

            Console.ReadLine();
            Console.WriteLine("Hello World!");
        }
    }
}
