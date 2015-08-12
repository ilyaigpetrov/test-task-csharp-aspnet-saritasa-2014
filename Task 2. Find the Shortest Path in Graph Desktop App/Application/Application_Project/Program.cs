using System;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Collections.Generic;
using SaritasaShortestPath.ShortestPathSolver;
using SaritasaShortestPath.Dijkstra;
using System.Xml;
using System.IO;

namespace SaritasaShortestPath
{

    public class Program
    {

        static Program()
        {
            // Make this program emit english error messages only.
            System.Threading.Thread.CurrentThread.CurrentCulture
                = System.Threading.Thread.CurrentThread.CurrentUICulture
                = System.Globalization.CultureInfo.InvariantCulture;
        }

        private static IEnumerable<string> FindShortestPathFor(ParseResult parseResult)
        {
            return DijkstraAlgorithm.Run(
                    parseResult.Start,
                    parseResult.Finish,
                    parseResult.AllButCrashedNodes
                )
                .Cast<Node>().Select(n => n.GetId());
        }

        /* Public */

        public static IEnumerable<string> FindShortestPathForXmlText(string inputXmlText)
        {
            var parseResult = XmlGraphParser.ParseGraphDescriptionFromXmlText(inputXmlText);
            return FindShortestPathFor(parseResult);
        }

        public static IEnumerable<string> FindShortestPathForXmlFile(string inputXmlPath)
        {
            var parseResult = XmlGraphParser.ParseGraphDescriptionFromXmlFile(inputXmlPath);
            return FindShortestPathFor(parseResult);
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                System.Console.WriteLine("Usage: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName + " <path to road_system.xml file>");
                return 1;
            }
            try
            {
                string inputXmlPath = args[0];

                var idsPath = FindShortestPathForXmlFile(inputXmlPath);
                var result = String.Join(" ->\r\n", idsPath);
                if (result.Length == 0)
                    System.Console.WriteLine("No route found.");
                else
                    System.Console.WriteLine(result);

                return 0;
            }
            catch (ClientSafeException e)
            {
                System.Console.WriteLine(e.Message);
            }
            catch (Exception)
            {
                System.Console.WriteLine("A mysterious error happened in some deep inner workings of this program. Sorry.");
            }
            return 1;
        }
    }
}
