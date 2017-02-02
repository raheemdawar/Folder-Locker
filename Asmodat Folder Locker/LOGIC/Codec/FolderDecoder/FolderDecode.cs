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
    public partial class FolderDecoder
    {
        
        public void Decode(string[] folders, SecureString password = null, bool killLockers = false)
        {
            if (folders.IsNullOrEmpty())
                return;

            Methods.JoinAll();

            IsBusy = true;
            Counter.Reset();
            Counter.Total = folders.Length;

            folders = folders.SortDescending(s => s.CountChars('\\'))?.ToArray();

            string parentPrevious = null, parentCurrent;
            foreach (string folder in folders)
            {
                parentCurrent = Directories.GetParent(folder);
                if (parentCurrent.IsNullOrEmpty() || !parentPrevious.IsNullOrEmpty() || parentCurrent != parentPrevious)
                {
                    Methods.JoinAll();
                    parentPrevious = parentCurrent;
                }

                Methods.Run(() => DecodeAndCount(folder, password, killLockers));
            }

            Methods.JoinAll();
            IsBusy = false;
        }

        private bool DecodeAndCount(string folder, SecureString password = null, bool killLockers = false)
        {
            bool result = Decode(folder, password, killLockers);
            Counter.Success += result ? 1 : 0;
            ++Counter.Compleated;
            return result;
        }

        private bool Decode(string folder, SecureString password = null, bool killLockers = false)
        {
            if (this.Stop) return false;

            //directory must exist and it cannot be a root folder
            if (Directories.IsRoot(folder) != false)
                return false;

            string name = Path.GetFileName(folder); //returns folder name
            string parent = Directories.GetParent(folder);
            string config = folder + "\\" + FolderConfigFileName;

            if (!name.ToLower().StartsWith("@") || !Files.Exists(config) || parent == null)
                return false;

            byte[] data = FileStreamEx.TryReadAll(config);

            if (data.IsNullOrEmpty())
                return false;
            
            Mode mode = (Mode)Int32Ex.FromBytes(data, 4);

            if (mode != Mode.None && mode != Mode.NonePassword)
                return false;


            string oldName = (mode == Mode.None) ? data.GetStringDecoded(8)?.UnGZip() : AES256.Decrypt(data.GetStringDecoded(8), password.Release());

            if (oldName.IsNullOrEmpty())
                return false;

            string oldFolder = parent + "\\" + oldName;
            config = oldFolder + "\\" + FolderConfigFileName;

            if (Directories.TryMove(folder, oldFolder, killLockers))
            {
                Files.Delete(config);
                return true;
            }
            else
                return false;
        }
        


    }
}
