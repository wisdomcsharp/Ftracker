using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;


namespace Filetracker_test
{
    class AuthorizationRules
    {
        FileInfo file;

        ///<summary>
        ///A List of AccessTypes, which this application knowns. Anything else, is sorted and added as one of the above or as special
        ///</summary>
        public enum AccessType
        {
            FullControl,
            Modify,
            ReadAndExecute,
            Read,
            Write,
            SpecialRight
        }
        public static IdentityReference ownerRef = null;
        public static Type ownerType = null;

        public AuthorizationRules(FileInfo file)
        {
            this.file = file;
        }

        ///<summary>
        ///Obtains an NTAccount name for a particular file
        ///</summary>
        private string owner()
        {
            if (ownerRef == null)
            {
                FileSecurity fs = file.GetAccessControl();
                IdentityReference ir = fs.GetOwner(typeof(NTAccount));
                ownerRef = ir;
            }
            return ownerRef.Value ;
        }
        public AuthorizationRuleCollection Auth()
        {
            // FileSecurity fs = file.GetAccessControl();

            AuthorizationRuleCollection ownerId = null;
            {

                try
                {
                    if(AuthorizationRules.ownerType == null)
                    {
                        AuthorizationRules.ownerType = Type.GetType("System.Security.Principal.SecurityIdentifier");//SecurityIdentifier

                    }
                    ///<summary>
                    ///This functions returns access control rules, for any given SID
                    ///</summary>
                    ownerId = file.GetAccessControl().GetAccessRules(false, true, AuthorizationRules.ownerType);

                }
                catch (Exception ex)
                {
                    ownerId = null;
                    Directories.errors.Add(new String[] { file.ToString(),"File","Couldn't get SID", ex.Message});
                }

                return ownerId;
            }
        }

        ///<summary>
        ///Check whether permissions were inherited or not.
        ///</summary>
        public bool? Inheritance()
        {
            bool? result = false;
            AuthorizationRuleCollection Authy = Auth();

            if (Authy != null)
            {
                foreach (FileSystemAccessRule rule in Auth())
                {
                    ///<summary>
                    ///For each rule, obtain SID for who this rule belongs to. Now check if the rule is inherited or not
                    ///</summary>
                    if (owner() == rule.IdentityReference.ToString())
                    {
                        result = rule.IsInherited;
                    }
                }
            }
            else
            {
                result = null;// if an error ocurred
            }

            return result;
        }




        ///<summary>
        ///Converts permissions, into a uknown, logged group of 5 main permission.
        ///These permission are listed above
        ///
        /// if a permission provides access to more than one ability, then both of those abilties are returned.
        ///</summary>
        public List<AccessType> getAccessTypes(String permission)
        {
            ///Based on
            ///https://msdn.microsoft.com/en-us/library/bb727008.aspx
            ///https://msdn.microsoft.com/en-us/library/system.security.accesscontrol.filesystemrights(v=vs.110).aspx
            ///

            List<AccessType> access = new List<AccessType>();

            switch (permission.Trim().ToLower())
            {
                case "fullcontrol": //8
                    access.Add(AccessType.FullControl);
                    access.Add(AccessType.Modify);
                    access.Add(AccessType.Read);
                    access.Add(AccessType.Write);
                    access.Add(AccessType.ReadAndExecute);
                    break;
                case "modify": //10
                    access.Add(AccessType.Modify);
                    access.Add(AccessType.Read);
                    access.Add(AccessType.Write);
                    access.Add(AccessType.ReadAndExecute);
                    break;
                case "readandexecute": //11
                    access.Add(AccessType.Read);
                    access.Add(AccessType.ReadAndExecute);
                    break;
                case "read": //12
                    access.Add(AccessType.Read);
                    break;
                case "write"://20
                    access.Add(AccessType.Write);
                    break;

                case "appenddata": //1
                case "changepermissions": //2
                case "createdirectories"://3
                case "createfiles": //4
                case "delete": //5
                case "deletesubdirectoriesandfiles": //6
                case "executefile"://7
                case "listdirectory": //9 [8]
                case "readattributes"://13 [9]
                case "readdata"://14 [10]
                case "readextendedattributes"://15 [11]
                case "readpermissions"://16 [12]
               // case "synchronize"://17 [13] [not part of special permissions]
                case "takeownership"://18 [14]
                case "traverse"://19 [15]
                case "writeattributes"://21 [16]
                case "writedata"://22 [17]
                case "writeextendedattributes"://23 [18]
                    access.Add(AccessType.SpecialRight);
                    break;

            }
            return access;
        }

    }
}