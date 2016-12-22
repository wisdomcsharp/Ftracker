using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Filetracker_test
{
    class Cryptography
    {
        String data;

        ///<summary>
        ///Set a value which needs to be hashed.
        ///</summary>
        public Cryptography(String data)
        {
            this.data = data;
        }


        ///<summary>
        ///Returns String SHA-1 hash
        ///</summary>
        public String toSha1()
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-","");
        }
    }
}
