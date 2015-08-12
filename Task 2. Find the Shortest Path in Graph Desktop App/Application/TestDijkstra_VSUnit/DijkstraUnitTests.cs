using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaritasaShortestPath.ShortestPathSolver;
using System.Linq;
using System.Collections.Generic;

namespace TestDijkstra_VSUnit
{

    /* Test Assistance */

    public class Node : INode
    {
        public readonly List<ILink> Links = new List<ILink>();
        public readonly string Id;
        public Node(string id)
        {
            this.Id = id;
        }
        public IEnumerable<ILink> GetLinks()
        {
            return Links.AsEnumerable();
        }
        public void AddLink(Node toNode, int weight)
        {
            Links.Add( new Link(toNode, weight) );
        }
        public override string ToString()
        {
            return this.Id;
        }
    }

    public class Link : ILink
    {
        public INode Neighbour { get; private set; }
        public int Weight { get; private set; }
        public Link(Node neighbour, int weight)
        {
            this.Neighbour = neighbour;
            this.Weight = weight;
        }
    }

    public class Graph
    {
        public readonly Node Start;
        public readonly Node Finish;
        public readonly Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
        public IEnumerable<Node> AllNodes
        {
            get { return this.Nodes.Select(p => p.Value).AsEnumerable(); }
        }
        private Node GetOrCreate(string id)
        {
            Node node;
            Nodes.TryGetValue(id, out node);
            if (node == null)
            {
                node = new Node(id);
                Nodes.Add(id, node);
            }
            return node;
        }

        public Graph(string description)
        {
            var sentences = description.Trim().Split(new string[] { "\r\n", "\n", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var start = sentences.First().Trim();
            var finish = sentences.Last().Trim();
            sentences.RemoveAt(0);
            sentences.RemoveAt(sentences.Count - 1);
            this.Start = GetOrCreate(start);
            this.Finish = GetOrCreate(finish);
            foreach (var edge in sentences)
            {
                var triple = edge.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var from = triple[0];
                var weight = int.Parse( triple[1].Substring(1) );
                var to = triple[2];
                var fromN = GetOrCreate(from);
                var toN = GetOrCreate(to);
                fromN.AddLink(toN, weight);
                toN.AddLink(fromN, weight);
            }
        }

        public IEnumerable<Node> ProducePath(string path)
        {
            var strings = path.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return ProducePath(strings);
        }

        public IEnumerable<Node> ProducePath(string[] path)
        {
            return path.Select(id => {
                Node node;
                this.Nodes.TryGetValue(id, out node);
                if (node == null)
                    throw new ArgumentOutOfRangeException("The node with id "+id+" is not in the graph.");
                return node;
            });
        }
    }


    [TestClass]
    public class DijkstraUnitTests
    {

        static void AssertPathsAreEqual(IEnumerable<INode> expected, IEnumerable<INode> actual)
        {
            var acount = actual.Count();
            var ecount = expected.Count();
            var minCount = acount;
            if (acount != ecount)
                minCount = Math.Min(acount, ecount);
            for (int i = 0; i < minCount; i++)
            {
                var actualN = actual.ElementAt(i);
                var expectedN = expected.ElementAt(i);
                Assert.AreEqual(expectedN, actualN,
                    "Paths diverge at index {0}: was {1}, but {2} expected.", i, actualN, expectedN);
            }
            Assert.AreEqual(ecount, acount, "Actual path's size is wrong, but it's elements are ok, if any.");
        }

        static void AssertSolutionIsCorrect(string graphDescription, string expectedPath)
        {
            var g = new Graph(graphDescription);
            var expected = g.ProducePath(expectedPath);
            var actual = SaritasaShortestPath.Dijkstra.DijkstraAlgorithm.Run(g.Start, g.Finish, g.AllNodes);
            AssertPathsAreEqual(expected, actual);
        }

        /* Test Methods */

        [TestMethod]
        public void TestBasicGraph()
        {
            AssertSolutionIsCorrect(
                @"
                1
                1 w12 2
                1 w22 3
                5 w33 6
                3 w55 5
                6
                ",
                "1 3 5 6");
            }

        [TestMethod]
        public void TestCycledGraph()
        {
            AssertSolutionIsCorrect(
                @"
                1
                1 w12 2
                1 w22 3
                2 w33 4
                3 w55 4
                4 w11 5
                5
                ",
                "1 2 4 5"
            );
        }

        [TestMethod]
        public void TestMinimalGraph()
        {
            AssertSolutionIsCorrect("1, 1 w42 2, 2", "1 2");
        }

        [TestMethod]
        public void TestDegenerateGraph()
        {
            AssertSolutionIsCorrect("1, 1 w0 1, 1", "1");
            AssertSolutionIsCorrect("1, 1", "1");
        }

        [TestMethod]
        public void TestNoRoute()
        {
            AssertSolutionIsCorrect("1, 2", "");
            AssertSolutionIsCorrect("1, 1 w0 6, 2 w0 8, 99", "");
        }

    }
}
