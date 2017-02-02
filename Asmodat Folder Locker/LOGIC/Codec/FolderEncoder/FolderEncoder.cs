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
        public void Encode(string[] folders, SecureString password = null, bool killLockers = false)
        {
            if (folders.IsNullOrEmpty())
                return;

            Methods.JoinAll();

            IsBusy = true;
            Counter.Reset();
            Counter.Total = folders.Length;
            GeneratedFolderNames.Clear();

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

                
                Methods.Run(() => EncodeAndCount(folder, password, killLockers));
                
            }

            Methods.JoinAll();
            IsBusy = false;
        }

        private bool EncodeAndCount(string folder, SecureString password = null, bool killLockers = false)
        {
            bool result = Encode(folder, password, killLockers);
            Counter.Success += result ? 1 : 0;
            ++Counter.Compleated;
            return result;
        }


        private bool Encode(string folder, SecureString password = null, bool killLockers = false)
        {
            if (this.Stop) return false;

            //directory must exist and it cannot be a root folder
            if (Directories.IsRoot(folder) != false)
                return false;

            string name = Path.GetFileName(folder); //returns folder name
            string parent = Directories.GetParent(folder);
            string config = folder + "\\" + FolderConfigFileName;

            if ((name.ToLower().StartsWith("@") && Files.Exists(config)) || parent == null)
                return false;

            if (!Files.Delete(config))
                return false;

            Files.Create(config);
            if (!Files.Exists(config))
                return false;

            string newFolder = this.GenerateNewFolderName(folder);

            if (newFolder.IsNullOrEmpty())
                return false;

            string pass = password.Release();

            Mode mode = pass.IsNullOrEmpty() ? Mode.None : Mode.NonePassword;

            List<byte> buffer = new List<byte>();
            buffer.AddRange(Int32Ex.ToBytes((int)(pass.IsNullOrEmpty() ? Mode.None : Mode.NonePassword))); //encode mode
            buffer.AddRange(((mode == Mode.NonePassword) ? AES256.Encrypt(name, pass) : name.GZip()).GetBytesEncoded()); //encode name
            buffer.AddToStart(Int32Ex.ToBytes(buffer.Count)); //encode count to start

            FileStream fs = FileInfoEx.TryOpen(config, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if (fs == null)
                return false;

            try
            {
                //write data
                if (!fs.TryWrite(buffer, 0)) 
                    return false;
            }
            finally
            {
                fs?.Flush();
                fs?.Close();
            }

            if (Directories.TryMove(folder, newFolder, killLockers))
                return true;
            else
                return false;
        }
    }
}
