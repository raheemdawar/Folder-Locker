using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.IO;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
using Asmodat.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static Asmodat_File_Lock.Codec;

namespace Asmodat_File_Lock
{
    public partial class FolderEncoder
    {

        public bool IsFolderEncoded(string folder)
        {
            if (folder.IsNullOrEmpty())
                return false;

            string name = Directories.GetName(folder);
            string parent = Directories.GetParent(folder);
            string config = folder + "\\" + FolderConfigFileName;

            if (parent.IsNullOrEmpty() || name.IsNullOrEmpty() || name[0] != '@' || !Files.Exists(config))
                return false;

            name = name.GetLast(name.Length - 1);

            ulong test;
            bool result = ulong.TryParse(name, System.Globalization.NumberStyles.HexNumber, null, out test);
            return result;
        }


        /// <summary>
        /// This list helps to prevent name duplication during multithreading encryption and foldername generation
        /// Should be cleared before executing mulithread encryption command
        /// </summary>
        private ThreadedList<string> GeneratedFolderNames = new ThreadedList<string>();

        private string GenerateNewFolderName(string folder)
        {
            if (folder.IsNullOrEmpty())
                return null;

            string parent = Directories.GetParent(folder);
            string currentName = Directories.GetName(folder); 

            if (parent.IsNullOrEmpty() || currentName.IsNullOrEmpty())
                return null;

            int length = Math.Min(currentName.Length, this.NewFolderNameLengthMin);

            string result;

            do
            {
                result = parent + "\\@" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, length);
                length += (length < NewFolderNameLengthMax) ? 1 : 0;
            }
            while (!GeneratedFolderNames.AddDistinct(result) || Directory.Exists(result));

            return result;
        }

    }
}
