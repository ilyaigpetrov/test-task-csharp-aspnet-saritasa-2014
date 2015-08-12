using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using SaritasaShortestPath.ShortestPathSolver;

namespace SaritasaShortestPath
{
    /* ParseResult structure for every Graph Parser */
    internal struct ParseResult
    {
        internal readonly INode Start;
        internal readonly INode Finish;
        internal readonly IEnumerable<INode> AllButCrashedNodes;
        internal ParseResult(INode start, INode finish, IEnumerable<INode> allButCrashedNodes)
        {
            Start = start;
            Finish = finish;
            AllButCrashedNodes = allButCrashedNodes;
        }
    }

    internal class Link : ShortestPathSolver.ILink
    {
        public INode Neighbour { get; private set; }
        public int Weight { get; private set; }
        public Link(Node neighbour, string weight)
        {
            this.Neighbour = neighbour;
            this.Weight = int.Parse(weight);
        }
    }

    internal class Node : ShortestPathSolver.INode
    {
        public readonly XElement NodeElement;
        private readonly List<Node> AllNodes;
        public Node(XElement nodeE, List<Node> allNodes)
        {
            this.NodeElement = nodeE;
            this.AllNodes = allNodes;
        }
        public string GetId()
        {
            return (string)NodeElement.Attribute("id");
        }
        public IEnumerable<ILink> GetLinks()
        {
            var xlinks = NodeElement.Elements("link").Select(
                e => new { Reference = e.Attribute("ref").Value, Weight = e.Attribute("weight").Value }
            );
            var refs = xlinks.Select(x => x.Reference);
            var children = AllNodes.Where(n => refs.Contains(n.NodeElement.Attribute("id").Value));
            var links = children.Select(
                child =>
                {
                    return new Link(child, xlinks.Where(x => x.Reference == child.GetId()).Single().Weight);
                }
            );
            return links;
        }
    }

    /*
        XmlGraphReader is a wrapper around `XmlReader`
        charged with `XmlReaderSettings` to validate input xml
        as a graph description so you can catch validation errors
        while using `XmlReader`.
    */
    internal class XmlGraphReader
    {
        private readonly XmlReader _XmlReader;
        private static readonly XmlReaderSettings _XmlReaderSettings;
        static XmlGraphReader()
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema
            };
            var xsdSchemaAsText = SaritasaShortestPathRootNamespace.Properties.Resources.GraphXsdSchema;
            var schemas = new XmlSchemaSet();
            schemas.Add(null, XmlReader.Create(new StringReader(xsdSchemaAsText)));
            settings.Schemas = schemas;
            _XmlReaderSettings = settings;
        }
        private XmlGraphReader(XmlReader validatingXReader)
        {
            this._XmlReader = validatingXReader;
        }
        public XmlReader GetValidatingXmlReader()
        {
            return this._XmlReader;
        }
        public static XmlGraphReader CreateFromXmlText(string inputXmlText)
        {
            var xReader = XmlReader.Create(new StringReader(inputXmlText), _XmlReaderSettings);
            return new XmlGraphReader(xReader);
        }
        public static XmlGraphReader CreateFromXmlFile(string inputXmlPath)
        {
            var xReader = XmlReader.Create(inputXmlPath, _XmlReaderSettings);
            return new XmlGraphReader(xReader);
        }
    }

    internal class XmlGraphParser
    {
        /*
            Input XDocument is supposed to be almost correct.
            These checks are perfomed:
                - There is single start node.
                - There is single finish node.
            If XDocument incorrect in some ways passes these checks
            the behaviour is not determined.

            The following graph errors are known not to distort the desired behaviour:
                - If links of one edge has different weights then they
                  can be considered as two directed edges and Dijkstra algorithm
                  is supposed to work correctly in such a case.
                - If the graph has one-way links then it is a directed graph, same as
                  above.
        */
        private static ParseResult ParseGraphDescriptionFromXDocument(XDocument xDoc)
        {
            var root = xDoc.Root;
            var nodes = root.Elements("node")
                .Where(
                    // Filter out crashed nodes with status=crash (the only possible value).
                    e => e.Attribute("status") == null
                );
            List<Node> allButCrashedNodes = new List<Node>();
            foreach (var node in nodes)
                allButCrashedNodes.Add(new Node(node, allButCrashedNodes));

            Node start, finish;
            try
            {
                start = allButCrashedNodes.Where(n => (string)n.NodeElement.Attribute("role") == "start").Single();
                finish = allButCrashedNodes.Where(n => (string)n.NodeElement.Attribute("role") == "finish").Single();
            }
            catch (InvalidOperationException ex)
            {
                throw new inputValidationSafeException("There is no start or finish node in the xml supplied.", ex);
            }

            return new ParseResult(start, finish, allButCrashedNodes);
        }

        private static ParseResult ParseGraphDescriptionWithXmlGraphReader(XmlGraphReader graphReader)
        {
            XDocument xDoc;
            try
            {
                var xmlReader = graphReader.GetValidatingXmlReader();
                xDoc = XDocument.Load(xmlReader);
            }
            catch (System.Xml.XmlException ex)
            {
                throw new inputValidationSafeException("The structure of the xml input is not correct. Is it an xml?", ex);
            }
            catch (System.Xml.Schema.XmlSchemaValidationException ex)
            {
                var message = String.Format(
                    "The xml input failed to pass validation.\r\nDetails:\r\nline {0}, column {1}\r\n{2}",
                    ex.LineNumber,
                    ex.LinePosition,
                    ex.Message
                );
                throw new inputValidationSafeException(message, ex);
            }
            return ParseGraphDescriptionFromXDocument(xDoc);
        }

        /* Public Parsers */

        public static ParseResult ParseGraphDescriptionFromXmlText(string inputXmlText)
        {
            var graphReader = XmlGraphReader.CreateFromXmlText(inputXmlText);
            return ParseGraphDescriptionWithXmlGraphReader(graphReader);
        }

        public static ParseResult ParseGraphDescriptionFromXmlFile(string inputXmlPath)
        {
            XmlGraphReader graphReader;
            try
            {
                graphReader = XmlGraphReader.CreateFromXmlFile(inputXmlPath);
                return ParseGraphDescriptionWithXmlGraphReader(graphReader);
            }
            catch (Exception ex)
            {
                var safeReadingEx = new FileReadingSafeException("Error occured while accessing and parsing the file: \"" + inputXmlPath + "\".", ex);
                try
                {
                    throw;
                }
                catch (System.IO.IOException)
                {
                    safeReadingEx.AugmentMessageWithLine("The file or directory wasn't found.");
                }
                catch (System.Security.SecurityException)
                {
                    safeReadingEx.AugmentMessageWithLine("Program has no permissions to access the file.");
                }
                catch (Exception e)
                {
                    if (e is ArgumentException || e is UriFormatException)
                        safeReadingEx.AugmentMessageWithLine("The file path format is malformed.");
                }
                throw safeReadingEx;
            }
        }
    }
}
