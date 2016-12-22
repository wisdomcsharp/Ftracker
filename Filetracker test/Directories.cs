using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public static class Directories
{
    public static Object _lock = new object();
    private static List<String> directory = new List<string>();//list of files to scan
    private static List<String> scannedDirectory = new List<string>(); //list of files scanned
    public static List<String[]> errors = new List<String[]>();//list of arrays. [dir url/file url][error name]

    public static int Requests = 0; //all the time we've requested a directory. For perforamnce, if this value increases, we know a thread has accessed.  
    public static int totalFiles = 0; //total amount of directory
    public static int savedFiles = 0;
    public static DateTime lastActivity = new DateTime(); //last time a thread access this

    ///<summary>
    ///Add any directories which needs to be scanned.
    ///</summary>
    public static void add(String dir)
    {
        lock (_lock)
        {
            directory.Add(dir);
            totalFiles = directory.Count /* - 1*/;
        }
    }

    ///<summary>
    ///Add files after being scanned. 
    ///</summary>
    public static void addScanned(String file)
    {
          if (!scannedContains(file))
            {
                scannedDirectory.Add(file);
            }
         
    }

    ///<summary>
    ///Checks whether a file has already been scanned. This is thread-safe
    ///</summary>
    public static bool scannedContains(String file)
    {
        bool a = false;
        lock (_lock)
        {
            a = Directories.scannedDirectory.Contains(file);
        }
        return a;
    }

    ///<summary>
    ///Count of total errors which has been logged, but not been saved.
    ///</summary>
    public static int getErrors()
    {//get total errors
        return errors.Count;
    }

    ///<summary>
    ///Returns an error array of logged errors. 
    ///</summary>
    public static String[] getErrorInfo()
    {
        String[] error = null;
        if (errors.Count > 0)
        {
            error = errors[0];
            errors.RemoveAt(0);
        }
        return error;
    }

    ///<summary>
    ///Obtains a directory which has not been scanned. This is thread-safe
    ///</summary>
    public static String get()
    {

        String dir = null;
        lock (_lock)
        {
            if (directory.Count > 0)
            {

                dir = directory[0];
                directory.RemoveAt(0);
                Requests++;
            }
        }
        return String.IsNullOrEmpty(dir) ? null : dir;
    }
}
