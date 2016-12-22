using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Security;

using System;
using System.Runtime.InteropServices;

namespace Filetracker_test
{
    public partial class Form1 : Form
    {


        public Form1(String[] args)
        {
            InitializeComponent();
            String inPath = "",
                   outPath = "",
                   name = "",
                   scanType = "all",
                   changeDate = default(DateTime).ToString(),
                   delimiter = ",",
                   wrap = "";
            
            bool anonymous = false;

            int streams = -1, //-1 == automatic
                maxTime = -1; //-1 == no time limit

            String Parameter = "";
            foreach (String a in args)
            {
                Parameter += a + " ";
                if (a.Split('=')[0].Trim().ToLower() == "inpath") //path
                {
                    inPath = a.Split('=')[1].Trim( ) +"\\";//the \\ is for a quick fix for C:\ <-- which craches the software unless C:\\ is used. Need to look into this
                }
                if (a.Split('=')[0].Trim().ToLower() == "streams") //parallel streams
                {
                    if (a.Split('=')[1].Trim() == "auto")
                    {
                        streams = -1;
                    }
                    else
                    {
                        streams = int.Parse(a.Split('=')[1].Trim());
                    }
                }
                if (a.Split('=')[0].Trim().ToLower() == "outpath") //output path
                {
                    outPath = a.Split('=')[1].Trim();
                }
                if (a.Split('=')[0].Trim().ToLower() == "name")
                {
                    name = a.Split('=')[1].Trim();
                }
                if (a.Split('=')[0].Trim().ToLower() == "scan") //scan type
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    scanType = !String.IsNullOrEmpty(data) ? data : "all";
                }
                if (a.Split('=')[0].Trim().ToLower() == "anonymous") //anonymous filenames
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    anonymous = data == "true" ? true : false;
                }
                if (a.Split('=')[0].Trim().ToLower() == "date") //change date
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    changeDate = !String.IsNullOrEmpty(data) ? data : default(DateTime).ToString();
                }
                if (a.Split('=')[0].Trim().ToLower() == "delimiter")
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    delimiter = !String.IsNullOrEmpty(data) ? data : ",";
                }
                if (a.Split('=')[0].Trim().ToLower() == "time") //max time
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    maxTime = !String.IsNullOrEmpty(data) ? int.Parse(data) : -1;
                }
                if (a.Split('=')[0].Trim().ToLower() == "wrap")
                {
                    String data = a.Split('=')[1].Trim().ToLower();
                    wrap = !String.IsNullOrEmpty(data) ? data : "";
                }
            }

            Parameter = Parameter.Trim();

            //start scan if minimum requirements are met
            if (!String.IsNullOrEmpty(inPath + outPath)) {
                new Thread((() =>
                {
                    new fileList(inPath, outPath, name, scanType, changeDate, delimiter, wrap, anonymous, streams, maxTime, Parameter).Start();
                })).Start();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {
            ///<summary>
            ///Update UI so the user knows whats going on within the program.
            ///</summary>
            Activitylabel.Text = "Last Activity: " + Directories.lastActivity.ToLongTimeString();
                Filelabel.Text = "Files: " + Directories.totalFiles;
                Processedlabel.Text = "Processed: " + Directories.Requests;
                Savedlabel.Text = "Saved Items: " + Directories.savedFiles;
                Errorlabel.Text = "Errors: " + Directories.getErrors();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("\"inpath=c:/\" //input path\r\n\"outpath=c:/\" //output scan directories\r\n\"streams=5\" //default is auto, there is no maximum. \r\n\"name=ABC\" //and tag which should be attached to output files  \r\n\"scan=all\" //1 = scan files, 2 =access list, all=do everything. the default is all\r\n\"anonymous=false\"  //true or false. the default is false\r\n\"date=20/12/2016\" //or 20-12-2016 must include day, month, year\r\n\"delimiter=,\" //default is comma\r\n\"wrap=\"\" //default is \" this set the character which wraps around object names, within output CSV files.\r\n\"time=-1\" //in minutes. default is -1, which is infinite, which means there's no max time\r\n\r\n--------------------------------------------------------------------------------------\r\n//Each stream can process approximately 7,500 - 15,000 files before reaching an StackOverflowException. To void this, simply increase the amount of streams or set its value as auto. By default the streams parameter is set to 100.\r\n--\r\n//INITIALIZATION TIME (SEC) - before a scan starts, we must first search the input directory, to collect absolute paths for every file that exists. INITIALIZATION TIME is the amount of seconds it took for this search to complete.\r\n--\r\n//START DATETIME - start time of the scan, when the program started reading each file\r\n--\r\n//ELAPSED TIME(SEC) - amount of seconds it took for the program to read every file\r\n--\r\n//END DATETIME - end time of the scan, when the program finnished, or stopped (because it didn't have anymore time left - \"max time\")\r\n--\r\n//SCANNED FILES - total amount of files scanned\r\n--\r\n//PARAMETER - passed parameter \r\n--\r\nNOTE * threads have to wait for each other to finish using a file, before they can use it, to write data. The time listed above, don't account for the amount of time (after reading every file), it took to write-out all those data to our output location.");
        }
    }
}