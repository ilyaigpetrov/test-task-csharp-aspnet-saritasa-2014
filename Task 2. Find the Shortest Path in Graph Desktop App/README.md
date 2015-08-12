The Task
========
Write a shortest path solver for input graph description in xml format.

The Solution
============
The solution uses XSD-schema for input validation,  
LINQ to XML for further validation and interaction with the graph.  
Dijkstra algorithm is used for finding the shortest path.  


Directory Structure
===================
```
.
│   README.txt // this file
│
├───Application // Contains projects which depend on each other.
│   ├───Application_Project   // The main project, depends on Dijkstra project.
│   │   │   CommonPublic.cs   // Common public classes used throughout the project.
│   │   │   Program.cs        // Contains `Main` method, used to produce `.exe`-file.
│   │   │   Program.csproj    // Project file, resembles Makefile, may be used with MSBuild or VS.
│   │   │   XmlGraphParser.cs // Parses input xml, validates it (using xsd schema), produces an abstract node-link set which may be used in Dijkstra algorithm.
│   │   │
│   │   ├───Inputs // Input files used for debugging.
│   │   │       Sample.xml
│   │   │
│   │   └───Resources
│   │           GraphSchema.xsd // XSD schema used for validating input xml graph descriptions.
│   │
│   ├───Dijkstra_Project // Dijkstra algorithm implementation.
│   │       Dijkstra.csproj
│   │       DijkstraAlgorithm.cs
│   │
│   └───TestDijkstra_VSUnit
│       │───DijkstraUnitTests.cs
│       └───TestDijkstra_VSUnit.csproj
│
├───Test_VSUnit_Project // Tests of the application main project.
│   ├───ApplicationUnitTests.cs
│   ├───TestApplication_VSUnit.csproj
│   └───Inputs
│           Sample.xml
│
└───_Solution // VS solution files.
        SaritasaShortestPathSolution.sln
```

Clarifications
--------------
Projects are put in one folder if they have dependencies.  
So,  
TestDijkstra_VSUnit depends on Dijkstra_Project,  
Application_Project depends on Dijkstra_Project.  
        
Dijkstra algorithm implementation is organized as a separate project so  
it may be tested independently of the main project.  

Unit testing requires methods under the test to be marked as public so excluding  
it for separate testing makes it possible to conceal main project implementation  
details (you can compile Dijkstra assembly into the main assembly, but still need  
further defense against disassembling anyway).  

It also makes it easier to substitute Dijkstra with another algorithm implementation
in future.
