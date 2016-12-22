##Ftracker - File system processor



###Description 
Ftracker allows you to scan subfolders to detect _duplicate files_, _inheritance_ and _access rights_. Once a scan has begun, CSV reports are produced, which creates 3 different reports:



####Access List 

>This file contains data such as _file-id_, _inheritance_, _user SID_, _full access status_, _modify status_, _read & execute status_, _read status_, _write status_, and _special rights_


####File List

> This file contains data such as filename, path, extension, creation date, modified date, last access date, file-id, duplicate-id



####Runtime

> This file contains information in regards to the current scan. This may include scan start _time_, _duration_, _total files_ ...



####Error

> This file contains different types of errors which were intercepted during scans.

***

###Features

* Scan files anonymously
* Set multi-threading
* Specify time limit
* Modify delimiter
* Customise column/text wrap


###Command Example

`@echo off`

`"Ftracker.exe" inpath="C:\Users" outpath="C:\Users\Someone\Documents\Visual Studio 2015\Projects\Filetracker - NET40\Filetracker test\bin\Release" anonymous=false streams=1000 time=-1 delimiter=, scan=all "wrap=""`

`exit`
