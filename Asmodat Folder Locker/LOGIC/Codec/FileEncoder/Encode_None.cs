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
    public partial class FileEncoder
    {
        private bool Encode_None(string file, SecureString password = null, bool killLockers = false)
        {
            if (this.Stop) return false;

            if (killLockers)
            {
                Files.TryKillLockingProcesses(file, 500);

                //setting attributes to normal is important otherwise you might not be able to rename or access your files
                FileInfoEx.TrySetAttributes(file, FileAttributes.Normal);
            }

            if (!FileInfoEx.CanReadWrite(file))
                return false;

            string oldFile = file;

            file = this.GenerateNewFileName(file);
            if (!Files.TryMove(oldFile, file))
                return false;

            List<byte> buff = new List<byte>();

            //save mode
            if (password == null)
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.None))); 
            else
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.NonePassword)));

            if (password != null) //encode filename with password
                buff.AddRange(AES256.Encrypt(Files.GetName(oldFile, true), password.Release()).GetBytesEncoded());
            else //don't encode filename
                buff.AddRange((Files.GetName(oldFile, true)).GetBytesEncoded());

            FileStream fs = FileInfoEx.TryOpen(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if (fs == null)
                return false;

            try
            {
                long fileSize = fs.Length;//save original size of the file
                int cutoutSize = (int)Math.Min(CutoutSizeMax, fileSize); //file size might be smaller then defined coutout

                buff.AddRange(Int32Ex.ToBytes(cutoutSize));
                buff.AddRange(fs.TryRead(0, cutoutSize));

                byte[] dataNew = new byte[Math.Max(cutoutSize, 8)];
                Array.Copy(Int64Ex.ToBytes(fileSize), dataNew, 8);

                if (!fs.TryWrite(ref dataNew, 0)) //write eof information to beggining of file
                    return false;

                if (!fs.TryWrite(buff, fs.Length)) //write data to the end of file
                    return false;
            }
            finally
            {
                fs.TryFlush();
                fs.TryClose();
            }

            return true;
        }
    }
}
