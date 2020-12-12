using System;
using System.Collections.Generic;
using System.IO;

namespace XMLSugar_Playground
{
    class Program
    {

        static void Main(string[] args)
        {
            var xml = new XMLSugar.XMLSugar();
            xml.LoadFromFile(@"C:\Users\miche\Documents\TFA\Commesse\ProAutomator\IO_ST_A.xml");

            //var item1 = xml.Access("SW.WatchAndForceTables.PlcWatchTable/AttributeList/Name");
            var items = xml.AccessAll("SW.WatchAndForceTables.PlcWatchTable/ObjectList/SW.WatchAndForceTables.PlcWatchTableEntry");

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
            xml.SaveToFile(@"C:\Users\miche\Documents\TFA\Commesse\ProAutomator\Generated\Globals\test.xml");

            Console.WriteLine("Done.");
            Console.WriteLine(Directory.GetCurrentDirectory());

            Console.ReadLine();
            Console.WriteLine("Hello World!");
        }
    }
}
