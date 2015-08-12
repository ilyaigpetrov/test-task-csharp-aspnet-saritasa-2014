Task ¹2:
Shortest Way
 
Task Description
 
There is a graph - road system. Each link has weight and each node has
status, role and id:
 
* Link weight.

This is the time taken to move from one node to another and vice
verse by car.

* Node status.

Only one possible value is "crash". If it's set then car cannot go to
that specific node (so it will be like:
  <node id="1" li="" status="crash"></node>
  <node id="1" li="" status="crash"></node>
)

* Node id.

The identifier for internal system usage.

* Node role can be either start or finish.

 
You need to find a shortest way from "start node" to "finish node":
 
1.  The car starts from start node and must find the optimal way (by time) to finish node.
2.  Consider that link can only have one weight. Back way takes the same time.

3.  Application must report any errors to user (incorrect xml, no route, no start node etc).
4.  Application should show user the shortest way as array of node id's.

5.  Application must parse xml file for input data (see below for file structure). Sample file see in attachment.
6.  Application reads the road system from external xml file.
 
 
XML File Structure
 
Below is a sample xml file that defines two nodes (with "1" and "2" id's) and link between them (weight = 5). Also it defines node id = 1 as start node and node id = 2 as finish node.

<graph>
  <node id="1">
    <link ref="2" weight="5">
  </node>
  <node id="2">
    <link ref="1" weight="5">
  </node>
</graph> 


Technical Requirements
 
1.  Visual Studio 2010 or 2012 solution (you can use express edition for free).
2.  User interface - WinForms or Console application. Interface language - English.
3.  Project must contain unit tests to test main functionality (you can use NUnit or Visual Studio Unit Testing Framework).
4.  Use object oriented approaches in development.
5.  All code must contain reasonable comments in English.

