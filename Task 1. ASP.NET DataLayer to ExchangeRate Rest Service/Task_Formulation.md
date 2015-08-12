Task ¹1:
Exchange Rates
 
Task Description
 
The application supposed to retrieve exchange rates for given
date range.
 
1.  UI should be a simple web form with two date pickers for
    start date and finish date and currency
    selector (RUB, EUR, USD, GBP, JPY) control.

2.  Use following web-service to get data for specific date
    range: http://openexchangerates.org/. Now it requires
    registration to use it. You can register for free here:
    https://openexchangerates.org/signup/free

3.  Data received from web service should be saved in database.

4.  Application should cache retrieved rates data in database
    and download rates for any other dates on demand. For
    instance if user selects data from 01/01 to 01/31 and then
    from 01/15 to 02/15 part of data will be retrieved from
    database (01/15 to 01/31) whereas other part (02/01 to 02/15)
    will from external source.

5.  Create chart that shows currency dynamic of change for specific period.

6.  Selected period should not exceed 2 months.

7.  Open Exchange Rates site now requires registration, but it
    allows free accounts (https://openexchangerates.org/signup/free).
    To get data from service you should download and parse day-by-day.
    To get data by dates range it requires paid account, you shouldn't
    do that.
 
Technical requirements
1.  Visual Studio 2010 or 2012 (you can use express edition for free).
2.  ASP.NET MVC.
3.  ORM - Entity Framework.
4.  DBMS - MS SQL Server 2008, MS SQL Server Compact or Local DB.
5.  Use any open source library to draw charts (jqPlot, Highcharts).
6.  Business layer must be separated from presentation layer and persistence layer.
    Its must be different namespaces.
7.  There are no specific design requirements for project but it must be comfortable for end user. Interface language - English.
8.  All code must contains reasonable comments in English.