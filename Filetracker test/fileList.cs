using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using System.Security.AccessControl;

namespace Filetracker_test
{
    class fileList
    {
        String inDirectory,
               outDirectory,
            outFile,
            outFileExtra,
               name,
               scanType,
               delimiter,
               parameter,
            wrap;

        DateTime changeDate;

        bool anonymous,
            outFileExists = false,
            outFileExtraExists = false,
            protocolProduced = false;

        int streams,
            maxTime;


        Stopwatch stopWatch = new Stopwatch(),
                  initializationWatch = new Stopwatch();

        DateTime startTime,
                 endTime;


        ///<summary>
        ///These fields are used to lock threads onto an object whilst data is being written
        ///</summary>
        private static Object rwlAccess = new Object();
        private static Object rwlFiles = new Object();
        private static Object rwlProtocol = new Object();

        ///<summary>
        ///Instantiate objects before scan
        ///</summary>
        public fileList(String inDirectory,
                        String outDirectory,
                        String name,
                        String scanType,
                        String changeDate,
                        String delimiter,
                        String wrap,
                        bool anonymous,
                        int streams,
                        int maxTime,
                        String parameter)
        {
            this.parameter = parameter;
            this.inDirectory = inDirectory;
            this.outDirectory = outDirectory;
            this.name = name;
            this.scanType = scanType;
            this.changeDate = !String.IsNullOrEmpty(changeDate) ? Convert.ToDateTime(changeDate) : default(DateTime);
            this.delimiter = !String.IsNullOrEmpty(delimiter) ? delimiter.Trim() : ",";
            this.anonymous = anonymous;
            this.streams = streams;
            this.maxTime = maxTime;
            this.wrap = wrap;
        }

        ///<summary>
        ///From a given directory, scans and obtains all sub-files 
        ///</summary>
        private static void addDirectories(string inDirectory)
        {
            //SET LAST activity 
            Directories.lastActivity = DateTime.Now;

            foreach (string file in Directory.GetFiles(Path.GetFullPath(inDirectory)))
            {
                Directories.add(file);
            }



            foreach (string subDir in Directory.GetDirectories(inDirectory))
            {
                try
                {
                    addDirectories(subDir);
                }
                catch (Exception ex)
                {
                    Directories.errors.Add(new String[] { subDir, "Directory", "Access Denied", ex.Message });
                    // swallow, log, whatever
                    //possibly log dir that we couldn't get

                    //read the specification, to see what should happen to directories which can't be accessed
                }
            }
        }

        ///<summary>
        ///Scans for files in inPath. Set Streams. Start StopWatch, then begins multi-thread Scan.
        ///</summary>
        public void Start()
        {
            initializationWatch.Start();
            addDirectories(inDirectory);
            initializationWatch.Stop();


            //set streams auto
            if (this.streams == -1)
            {
                this.streams = (Directories.totalFiles / 7500) + 1; //+1 for luck
            }

            stopWatch.Start();
            startTime = DateTime.Now;
         //   Task mainThread = new Task(() =>
         //  {
               //start threads
               for (int i = 0; i < streams; i++)
               {
                   Task a = new Task(() =>
                   {
                      scan();
                   });
                   a.Start();
               }
           //});
           // mainThread.Start();
        }
        //continue


        ///<summary>
        ///Scans files, if not, then publish summary of previous scans.
        ///</summary>
        private void scan()
        {
            restart:

            ///<summary>
            ///Makes an entry to last activity, so we know its active
            ///</summary>
            Directories.lastActivity = DateTime.Now;

            //check if directory needs scanning. If not exit.
            String file = Directories.get();
            bool continueScan = true;
            ///<summary>
            ///Checks whether file has already been scanned. if not, inform the thread to stop.
            ///</summary>
            if (Directories.scannedContains(file))
            {
               continueScan = false;
            }
            else
            {
                ///<summary>
                ///Add to scanned files list.
                ///</summary>
                Directories.addScanned(file);
            }

            //check if there's any file to scan. Check if scan == -1 (no time limit) or is below allowed time
            if (file != null && (maxTime == -1 || (stopWatch.ElapsedMilliseconds / 60000) < maxTime))
            { /*
                    //To prevent stackoverflow, sleep if there's too much unsaved data
                    while(Directories.Requests != 0 && Directories.savedFiles !=0 && (Directories.Requests - Directories.savedFiles) > 10000)
                    {

                    }
                    */

                ///<summary>
                ///Checks the sort of scan thats required again, before proceeding
                ///</summary>
                if (continueScan == true)
                {
                    if (this.scanType == "1")
                    {
                        //scan the file list
                        scanFileList(file);
                    }
                    if (this.scanType == "2")
                    {
                        //scan and check access permission
                        scanFileAccess(file);

                    }

                    if (this.scanType == "all")
                    {
                        scanAll(file);
                    }

                }

                ///<summary>
                ///Begins a new scan.
                ///</summary>
                goto restart;

            }
            else
            {
                ///<summary>
                ///When all scans are complete, stop watches and produce a Error log and protocol report files
                ///</summary>
                stopWatch.Stop();
                endTime = DateTime.Now;
                //Produce report 
                produceFileProtocol();
                Console.WriteLine("RunTime: nothing");

                //time to build protocol files. Check for errors and report them


            }; //gets a file




            //  

        }

        private void scanAll(String file)
        {
            FileInfo fileInfo = new FileInfo(file);
            //for optimization, check creation date before scanning
            if (this.changeDate <= fileInfo.CreationTime.Date)
            {
                String fileName = Path.GetFileNameWithoutExtension(file),
                       filePath = Path.GetDirectoryName(file),
                       fileExtension = Path.GetExtension(file).Replace(".", ""),
                       fileCreationDate = fileInfo.CreationTime.Date.ToShortDateString(),
                       fileModifiedDate = fileInfo.LastWriteTime.Date.ToShortDateString(),
                       fileAccessDate = fileInfo.LastAccessTime.Date.ToShortDateString(),
                       fileId = (new Cryptography(inDirectory + fileName + filePath + fileExtension)).toSha1(),
                       duplicateId = (new Cryptography(fileName + fileExtension + fileInfo.Length)).toSha1();
                bool? fileInherited = (new AuthorizationRules(fileInfo)).Inheritance();
                if (anonymous == true)
                {
                    fileName = fileName.Length > 1 ? fileName.Substring(0, 2) + "*****" : fileName + "*****";
                }
                if (String.IsNullOrEmpty(outFile))
                {
                    ///<summary>
                    ///Because date is used as part of file name, if we set these values
                    ///without checking if "IsNullOrEmpty" it'll create multiple files on the outPath
                    ///With different dates-times.
                    ///
                    ///We need results stored in one file
                    ///</summary>
                    DateTime localDate = DateTime.Now;
                    outFile = (outDirectory + "/FL-" + this.name + " ").Trim() + localDate
                        .ToString()
                        .Replace("/", "-")
                        .Replace(":", "-")
                        + ".csv";
                }


                ///<summary>
                ///Lock object whilst data is been written to file
                ///</summary>
                lock (rwlFiles)
                {
                    //Check if file has already been created 
                    if (outFileExists == true)
                    {

                        Write(outFile,
                             String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}{1} {0}{10}{0}" + Environment.NewLine, wrap, delimiter, fileName, filePath, fileExtension, fileCreationDate, fileModifiedDate, fileAccessDate, fileId, duplicateId, fileInherited == null ? "ERROR" : fileInherited.ToString()));
                    }
                    else
                    {
                        Write(outFile,
                            String.Format("{0}FILENAME{0}{1} {0}PATH{0}{1} {0}EXTENSION{0}{1} {0}CREATION DATE{0}{1} {0}MODIFIED DATE{0}{1} {0}LAST ACCESS DATE{0}{1} {0}FILE-ID{0}{1} {0}DUPLICATE-ID{0}{1} {0}INHERIT{0}" + Environment.NewLine, wrap, delimiter) +
                            String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}{1} {0}{10}{0}" + Environment.NewLine, wrap, delimiter, fileName, filePath, fileExtension, fileCreationDate, fileModifiedDate, fileAccessDate, fileId, duplicateId, fileInherited == null ? "ERROR" : fileInherited.ToString()));
                        outFileExists = true;
                    }
                }


                //AF
                String outResult = "";

                try
                {
                    ///<summary>
                    ///Loops through a list of authorization rules for each user
                    ///</summary>
                    AuthorizationRules AuthRules = new AuthorizationRules(fileInfo);
                    foreach (FileSystemAccessRule rule in AuthRules.Auth())
                    {

                        List<AuthorizationRules.AccessType> rights = new List<AuthorizationRules.AccessType>();
                        foreach (String unknownRight in rule.FileSystemRights.ToString().Split(','))
                        {
                            ///<summary>
                            ///Loops through a list of rights, pass it to the "getAccessTypes" function, 
                            ///which determines the sort of right it is.
                            ///
                            ///
                            ///</summary>
                            rights.AddRange(AuthRules.getAccessTypes(unknownRight));
                        }

                        Dictionary<String, Boolean> checkRights = new Dictionary<string, bool>
                        {
                            { "fullcontrol", false},
                            { "modify", false},
                            {"readandexecute", false },
                            {"read", false},
                            {"write", false},
                            {"specialright", false}
                        };

                        ///<summary>
                        ///Go through all the rights that has been collected, convert them to string (lower case),
                        ///and change "checkRights" false values into true, if we detect a Right. If not,
                        ///It'll remain false.
                        ///</summary>
                        foreach (AuthorizationRules.AccessType aT in rights)
                        {
                            checkRights[aT.ToString().ToLower()] = true;
                        }

                        outResult += String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}" + Environment.NewLine, this.wrap, this.delimiter, fileId, rule.IdentityReference,
                            checkRights["fullcontrol"], checkRights["modify"], checkRights["readandexecute"], checkRights["read"], checkRights["write"], checkRights["specialright"]);

                    }

                    //Write file to output
                    if (String.IsNullOrEmpty(outFileExtra))
                    {
                        ///<summary>
                        ///Because date is used as part of file name, if we set these values
                        ///without checking if "IsNullOrEmpty" it'll create multiple files on the outPath
                        ///With different dates-times.
                        ///
                        ///We need results stored in one file
                        ///</summary>
                        DateTime localDate = DateTime.Now;
                        outFileExtra = (outDirectory + "/AL-" + this.name + " ").Trim() + localDate
                            .ToString()
                            .Replace("/", "-")
                            .Replace(":", "-")
                            + ".csv";
                    }

                    ///<summary>
                    ///Lock object whilst data is been written to file
                    ///</summary>
                    lock (rwlAccess)
                    {
                        if (outFileExtraExists == true)
                        {
                            //outResult
                            Write(outFileExtra, outResult);
                        }
                        else
                        {
                            //outResult
                            Write(outFileExtra, String.Format("{0}FILE-ID{0}{1} {0}ACCESS-ID{0}{1} {0}FULL ACCESS{0}{1} {0}MODIFY{0}{1} {0}READ & EXECUTE{0}{1} {0}READ{0}{1} {0}WRITE{0}{1} {0}SPECIAL RIGHT{0}" + Environment.NewLine, this.wrap, this.delimiter) + outResult);
                            outFileExtraExists = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //file   //ex
                    //crashed for some reason... whilst checking file access data (returned null)
                }



            }
        }

        private void scanFileList(String file)
        {
            FileInfo fileInfo = new FileInfo(file);
            //for optimization, check creation date before scanning
            if (this.changeDate <= fileInfo.CreationTime.Date)
            {
                String fileName = Path.GetFileNameWithoutExtension(file),
                       filePath = Path.GetDirectoryName(file),
                       fileExtension = Path.GetExtension(file).Replace(".", ""),
                       fileCreationDate = fileInfo.CreationTime.Date.ToShortDateString(),
                       fileModifiedDate = fileInfo.LastWriteTime.Date.ToShortDateString(),
                       fileAccessDate = fileInfo.LastAccessTime.Date.ToShortDateString(),
                       fileId = (new Cryptography(inDirectory + fileName + filePath + fileExtension)).toSha1(),
                       duplicateId = (new Cryptography(fileName + fileExtension + fileInfo.Length)).toSha1();
                bool? fileInherited = (new AuthorizationRules(fileInfo)).Inheritance();
                if (anonymous == true)
                {
                    fileName = fileName.Length > 1 ? fileName.Substring(0, 2) + "*****" : fileName + "*****";
                }
                if (String.IsNullOrEmpty(outFile))
                {
                    ///<summary>
                    ///Because date is used as part of file name, if we set these values
                    ///without checking if "IsNullOrEmpty" it'll create multiple files on the outPath
                    ///With different dates-times.
                    ///
                    ///We need results stored in one file
                    ///</summary>
                    DateTime localDate = DateTime.Now;
                    outFile = (outDirectory + "/FL-" + this.name + " ").Trim() + localDate
                        .ToString()
                        .Replace("/", "-")
                        .Replace(":", "-")
                        + ".csv";
                }

                ///<summary>
                ///Lock object whilst data is been written to file
                ///</summary>
                lock (rwlFiles)
                {
                    //Check if file has already been created 
                    if (outFileExists == true)
                    {

                        Write(outFile,
                             String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}{1} {0}{10}{0}" + Environment.NewLine, wrap, delimiter, fileName, filePath, fileExtension, fileCreationDate, fileModifiedDate, fileAccessDate, fileId, duplicateId, fileInherited == null ? "ERROR" : fileInherited.ToString()));
                    }
                    else
                    {
                        Write(outFile,
                            String.Format("{0}FILENAME{0}{1} {0}PATH{0}{1} {0}EXTENSION{0}{1} {0}CREATION DATE{0}{1} {0}MODIFIED DATE{0}{1} {0}LAST ACCESS DATE{0}{1} {0}FILE-ID{0}{1} {0}DUPLICATE-ID{0}{1} {0}INHERIT{0}" + Environment.NewLine, wrap, delimiter) +
                            String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}{1} {0}{10}{0}" + Environment.NewLine, wrap, delimiter, fileName, filePath, fileExtension, fileCreationDate, fileModifiedDate, fileAccessDate, fileId, duplicateId, fileInherited == null ? "ERROR" : fileInherited.ToString()));
                        outFileExists = true;
                    }
                }


            }
        }

        private void scanFileAccess(String file)
        {
            //go through every file, and check their permissions, then write mo fucker write  (once lock is obtained)
            FileInfo fileInfo = new FileInfo(file);
            //for optimization, check creation date before scanning
            if (this.changeDate <= fileInfo.CreationTime.Date)
            {
                //result that will be written out
                String outResult = "";

                try
                {

                    ///<summary>
                    ///Loops through a list of authorization rules for each user
                    ///</summary>
                    AuthorizationRules AuthRules = new AuthorizationRules(fileInfo);
                    foreach (FileSystemAccessRule rule in AuthRules.Auth())
                    {

                        List<AuthorizationRules.AccessType> rights = new List<AuthorizationRules.AccessType>();
                        foreach (String unknownRight in rule.FileSystemRights.ToString().Split(','))
                        {  ///<summary>
                           ///Loops through a list of rights, pass it to the "getAccessTypes" function, 
                           ///which determines the sort of right it is.
                           ///
                           ///
                           ///</summary>
                            rights.AddRange(AuthRules.getAccessTypes(unknownRight));
                        }

                        Dictionary<String, Boolean> checkRights = new Dictionary<string, bool>
                {
                    { "fullcontrol", false},
                    { "modify", false},
                    {"readandexecute", false },
                    {"read", false},
                    {"write", false},
                    {"specialright", false}
                };

                        ///<summary>
                        ///Go through all the rights that has been collected, convert them to string (lower case),
                        ///and change "checkRights" false values into true, if we detect a Right. If not,
                        ///It'll remain false.
                        ///</summary>
                        foreach (AuthorizationRules.AccessType aT in rights)
                        {
                            checkRights[aT.ToString().ToLower()] = true;
                        }



                        String fileName = Path.GetFileNameWithoutExtension(file),
                               filePath = Path.GetDirectoryName(file),
                               fileExtension = Path.GetExtension(file).Replace(".", ""),
                               fileId = (new Cryptography(inDirectory + fileName + filePath + fileExtension)).toSha1();

                        outResult += String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}{1} {0}{8}{0}{1} {0}{9}{0}" + Environment.NewLine, this.wrap, this.delimiter, fileId, rule.IdentityReference,
                            checkRights["fullcontrol"], checkRights["modify"], checkRights["readandexecute"], checkRights["read"], checkRights["write"], checkRights["specialright"]);

                    }

                    //Write file to output
                    if (String.IsNullOrEmpty(outFile))
                    {
                        ///<summary>
                        ///Because date is used as part of file name, if we set these values
                        ///without checking if "IsNullOrEmpty" it'll create multiple files on the outPath
                        ///With different dates-times.
                        ///
                        ///We need results stored in one file
                        ///</summary>
                        DateTime localDate = DateTime.Now;
                        outFile = (outDirectory + "/AL-" + this.name + " ").Trim() + localDate
                            .ToString()
                            .Replace("/", "-")
                            .Replace(":", "-")
                            + ".csv";
                    }

                    ///<summary>
                    ///Lock object whilst data is been written to file
                    ///</summary>
                    lock (rwlAccess)
                    {
                        if (outFileExists == true)
                        {
                            //outResult
                            Write(outFile, outResult);
                        }
                        else
                        {
                            //outResult
                            Write(outFile, String.Format("{0}FILE-ID{0}{1} {0}ACCESS-ID{0}{1} {0}FULL ACCESS{0}{1} {0}MODIFY{0}{1} {0}READ & EXECUTE{0}{1} {0}READ{0}{1} {0}WRITE{0}{1} {0}SPECIAL RIGHT{0}" + Environment.NewLine, this.wrap, this.delimiter) + outResult);
                            outFileExists = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //file   //ex
                    //crashed for some reason... whilst checking file access data (returned null)
                }





            }

        }

        ///<summary>
        ///Produce a protocol and error file
        ///</summary>
        private void produceFileProtocol()
        {
            //protocol with error logs


            //check if file alread exists, then write out data

            //date and time. 
            //PL-FAILED- (inheritance error, no access...)
            //PL-RUNTIME- (runtime info)

            //INITIALIZATION TIME
            //START DATETIME
            //ELAPSED TIME(SEC)
            //END DATETIME
            //SCANNED FILES
            //PARAMETER

            //FILE-ID
            //SCAN-NAME
            //TYPE  [error type = failed.. .. .. ]
            //MESSAGE

            lock (rwlProtocol)
            {
                if (protocolProduced == false)
                {
                    protocolProduced = true;
                    //Write file to output
                    String runtimeReport = (outDirectory + "/P-RUNTIME").Trim() + ".csv";
                    String runtimeError = (outDirectory + "/P-ERROR").Trim() + ".csv";

                    ///<summary>
                    ///Writes P-RUNTIME file, which contains a report about this scan.
                    ///</summary>
                    if (!File.Exists(runtimeReport))
                    {
                        Write(runtimeReport, String.Format("{0}INITIALIZATION TIME (SEC){0}{1} {0}START DATETIME{0}{1} {0}END DATETIME{0}{1} {0}ELAPSED TIME (SEC){0}{1} {0}SCANNED FILES{0}{1} {0}PARAMETER{0}" + Environment.NewLine, this.wrap, this.delimiter) +
                                             String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}" + Environment.NewLine, this.wrap, this.delimiter, (initializationWatch.ElapsedMilliseconds / 1000), startTime.ToString(), endTime.ToString(), (stopWatch.ElapsedMilliseconds / 1000), Directories.totalFiles, parameter));
                    }
                    else
                    {
                        Write(runtimeReport, String.Format("{0}{2}{0}{1} {0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{6}{0}{1} {0}{7}{0}" + Environment.NewLine, this.wrap, this.delimiter, (initializationWatch.ElapsedMilliseconds / 1000), startTime.ToString(), endTime.ToString(), (stopWatch.ElapsedMilliseconds / 1000), Directories.totalFiles, parameter));

                    }


                    ///<summary>
                    ///Write Error log, is any errors exists
                    ///</summary>
                    String[] errors = null;
                    while ((errors = Directories.getErrorInfo()) != null)
                    {
                        if (!File.Exists(runtimeError))
                        {
                            Write(runtimeError, String.Format("{0}TYPE{0}{1} {0}EXCEPTION{0}{1} {0}MESSAGE{0}{1} {0}LOCATION{0}" + Environment.NewLine, this.wrap, this.delimiter) +
                                                 String.Format("{0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{2}{0}" + Environment.NewLine, this.wrap, this.delimiter, errors[0], errors[1], errors[2], errors[3]));
                        }
                        else
                        {
                            Write(runtimeError, String.Format("{0}{3}{0}{1} {0}{4}{0}{1} {0}{5}{0}{1} {0}{2}{0}" + Environment.NewLine, this.wrap, this.delimiter, errors[0], errors[1], errors[2], errors[3]));

                        }
                    }


                }
            }

        }

        ///<summary>
        ///Write data to a given file. Increments saved files, and LastActivity
        ///</summary>
        private void Write(String Path, String Content)
        { //PLEASE OBTAIN LOCK BEFORE USING! DONT DOOM US ALL!!
            Directories.savedFiles++;
            File.AppendAllText(Path, Content);
            //LAST ACtivity
            Directories.lastActivity = DateTime.Now;

        }
    }

}