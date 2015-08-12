using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaritasaShortestPath;
using System.Linq;

namespace Test_VSUnit
{
    [TestClass]
    public class ApplicationUnitTests
    {

        static string InputsPath = "../../Inputs/";

        [TestMethod]
        public void TestBasicXmlSample()
        {
            var actual = Program.FindShortestPathForXmlFile(InputsPath + "Sample.xml");
            var expected = new string[] { "1", "6", "8", "9", "10" };
            CollectionAssert.AreEqual(expected, actual.ToArray(), "The path is incorrect.");
        }

        [TestMethod]
        [ExpectedException(
            typeof(inputValidationSafeException),
            "Wrong xml without start node was accepted.")
        ]
        public void TestGraphHasNoStartNode()
        {
            var inputXml =
@"
<graph>
	<node id='1' role='finish'>
        <link ref='2' weight='42'/>
    </node>
    <node id='2'>
        <link ref='1' weight='42'/>
    </node>
</graph>
";
            var actual = Program.FindShortestPathForXmlText(inputXml);
        }

        [TestMethod]
        public void TestGraphEdgeHasDifferentWeights()
        {
            var actual = Program.FindShortestPathForXmlText(
@"
<graph>
	<node id='1' role='start'>
        <link ref='2' weight='42'/>
        <link ref='3' weight='50'/>
    </node>
    <node id='2'>
        <link ref='1' weight='42'/>
        <link ref='4' weight='42'/>
    </node>
    <node id='3'>
        <link ref='2' weight='0'/>
        <link ref='4' weight='0'/>
    </node>
    <node id='4' role='finish'>
        <link ref='2' weight='42'/>
        <link ref='3' weight='42'/>
    </node>
</graph>
"
                );
            var expected = new string[] { "1", "3", "4" };
            CollectionAssert.AreEqual(expected, actual.ToArray(), "The path is incorrect.");
        }

        [TestMethod]
        public void TestGraphWithDirectedEdges()
        {
            var actual = Program.FindShortestPathForXmlText(
@"
<graph>
	<node id='1' role='start'>
        <link ref='3' weight='50'/>
    </node>
    <node id='2'>
        <link ref='1' weight='42'/>
        <link ref='4' weight='42'/>
    </node>
    <node id='3'>
        <link ref='2' weight='0'/>
        <link ref='4' weight='1'/>
    </node>
    <node id='4' role='finish'>
        <link ref='2' weight='42'/>
    </node>
</graph>
"
            );
            var expected = new string[] { "1", "3", "4" };
            CollectionAssert.AreEqual(expected, actual.ToArray(), "The path is incorrect.");
        }

        [TestMethod]
        public void TestNoPath()
        {
            var actual = Program.FindShortestPathForXmlText(
@"
<graph>
	<node id='1' role='start'>
        <link ref='1' weight='0'/>
    </node>
    <node id='2' role='finish'>
        <link ref='2' weight='0'/>
    </node>
</graph>
"
            );
            var expected = new string[] {};
            CollectionAssert.AreEqual(expected, actual.ToArray(), "The path is incorrect.");
        }


    }
}
