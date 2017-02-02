using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.IO;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
using Asmodat.Types;
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
    public partial class FileDecoder
    {
        /// <summary>
        /// This list helps to prevent name duplication during multithreading encryption and filename generation
        /// Should be cleared before executing mulithread encryption command
        /// </summary>
        private ThreadedList<string> GeneratedFileNames = new ThreadedList<string>();

        /// <summary>
        /// This method generates new random filename
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string GenerateNewFileName(string file)
        {
            if (file.IsNullOrEmpty())
                return null;

            string dir = Files.GetDirectory(file);

            if (!Directory.Exists(dir))
                return null;

            int length = Math.Min(Files.GetName(file, false).Length, this.NewFileNameLengthMin);
            string result;

            do
            {
                result = dir + "\\" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, length) + FileExtention;
                length += (length < NewFileNameLengthMax) ? 1 : 0;
            }
            while (!GeneratedFileNames.AddDistinct(result) || new FileInfo(result).Exists);
            
            return result;
        }
    }
}
