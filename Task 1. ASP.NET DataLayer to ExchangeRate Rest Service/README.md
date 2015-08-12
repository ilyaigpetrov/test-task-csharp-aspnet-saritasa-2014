The Task
========
The task is to write an exchange rate site:  
	- you put in a date range and a currency,  
	- and the site plots you exchange rates for the request.  
Other requirements:  
	- data for rates is retrieved from an external JSON service,  
	- site's db is populated with retrieved data, so the db is used like a cache.  


The Solution
============
The solution includes an Asp.net MVC project with a test project based on  
VisualStudio tesing library.  
Tools:  
	- VisualStudio 2013 for Web,  
	- Entity Framework 6,  
	- ASP.NET MVC 5.  


Apologies
=========
1)  
The code is not polished and contains lots of 'TODO:' marks.  
Polishing and testing the code is not a problem if I have  
more time to put in.  
But I see no point in polishing as the current solution just  
works and proves the concepts.  
2)  
I'm not a professional APS.NET MVC developer so some of my  
solutions may be out of the supposed way to do the things.  


Design Decisions
================
The Repository Pattern Hides External Resources
-----------------------------------------------
External JSON services are abstracted from controllers and other layers  
in a repository entity (so, the repository pattern is used).  
It allows to switch repository implementation one day to, f.e., one  
which doesn't use external services at all.  

An external service is named an external resource hereafter.  

Current implementation uses one external resource.  
If the external resource fails to serve the request an exception from  
a defined set is thrown. The set is defined in `ExchangeResources.cs`.  
This set may be used in repository implementations not backed up by  
external resources, so they may throw the same set.  
The set includes types:  
	- try_later exception (f.e., timeout is reached, resource is under maintainance),  
	- try_other_request (f.e., the resource has no such dates or currency),  
	- maintainance exception (f.e., the resource requires payment),  
	- resource_brokage exception (f.e., the resource is broken, sends 5xx),  
	- development exception (caused by mistakes in our code).  


Known Issues and Prospectives
=============================
Extra POST request could be eliminated
--------------------------------------
Current site's form for requesting rates for a date range  
uses POST method so we have to implement it in the controller.  
Instead it could use GET with an `action` defined on submit by  
javascript-code, so we may eliminate additional POST request.  

Currency Miss in the DB May Cause Inconsistency with the External Resource
--------------------------------------------------------------------------
For a given date the external resource replies with a batch of currencies.  
For the date requested some of the currencies retrieved may be alredy stored  
in the DB (from previous replies of the same date), but some may be missing  
(f.e., it was deleted from the DB or the resource replied in an inconsistent  
way adding one more currency in it's reply of the same date).  
Dates already available can't be saved one more time and should be filtered  
out before `SaveChanges` sends SQL-request.  
Currently it throws an exception. Current solution requirements:  
1) don't delete records from the DB,  
2) the resource replies must be consistent.  


Unit Tests
==========
Unit tests are spare.  
Those available test cases when the chosen external resource  
violates reply contract, replies with errors or when it is not  
reachable due to timeout or DNS failures.  


Debugging
=========
`Trace.WriteLine` is used throughout the code to write traces.  
Traces are written to 'Traces.log' file in the solution directory root.  
Additional info may be got from 'site_path/trace.axd' and glimpse panel  
configured in 'site_path/glimpse.axd'.  


TODO
====
1)  
> Business layer must be separated from presentation layer and persistence layer.  
Its must be different namespaces.

2)  
Get rid of Delete in views.  

