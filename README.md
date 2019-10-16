# Mantic Framework
[Setting up your data model](/wiki/Setup-Data-Model)<br/>

[Examples](Examples)<br/>

[Full API Docs](API)<br/>

[Development Guidlines](Development-Guidlines)

# Problem
While using ADO.NET is the most performant solution possible, it results in some pretty nasty looking code.
Additionally there can be some annoying conversions that have to be done (particularly `Datetime`). 
Microsoft realized this and created something called Entity Framework. Unfortunately, while very user friendly, this framework operates nowhere the performance level of ADO

# Solution
Mantic itself is a mix of Entity Framework and ADO.NET. It uses ADO.NET to interact with the database and maps the results to C# objects (similar to Entity Framework).
One of the reasons Entity is slow is that it must create the SQL object to C# object mappings at runtime, as well as generate all the queries.
Mantic does this work ahead of time by requiring classes be registered before they are used in mantic.
Additionally, when the database structure inevitably changes, the datamodel is the only thing that must be updated. Those added database fields will be available wherever that model is used in the code.

# Is Mantic right for you?
I designed Mantic to be used for ad hoc scripts and C# API's as singleton middleware (it only gets instantiated once).
If performance is key, then avoid Mantic. If working with Datasets larger than 500,000 rows, take a look at either ADO.NET or Entity Framework.

# Benchmarks

![Select 608 Rows](images/select_608.png)

![Select 642686 Rows](images/select_642686.png)

![Insert](images/insert.png)

![Query Stored Procedure](images/query_proc.png)

![Scalar Stored Procedure](images/scalar_proc.png) 

[Download Raw Data](data/mantic.xlsx)
