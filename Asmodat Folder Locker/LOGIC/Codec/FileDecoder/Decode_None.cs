using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Extensions;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.IO;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
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
        
        public bool Decode_All(string file, SecureString password = null, bool killLockers = false)
        {
            if (this.Stop) return false;

            if (killLockers)
                Files.TryKillLockingProcesses(file, 500);

            FileStream fs = FileInfoEx.TryOpen(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if (fs == null)
                return false;

            string fileName = null;
            try
            {
                long fileSize = fs.Length;//save original size of the file
                long fileSizeOriginal; //file size that final result will be cut into

                if (!Int64Ex.TryFromBytes(out fileSizeOriginal, fs.TryRead(0, 8)))
                    return false;

                long offset = Math.Max(fileSizeOriginal, 8); //offset cannot be smaller then 8 bytes
                if (!offset.InOpenInterval(0, fs.Length))
                    return false;

                var data = fs.TryRead(offset, (int)(fs.Length - offset));

                if (data.IsNullOrEmpty())
                    return false;

                Mode mode = (Mode)Int32Ex.FromBytes(data);

                if(mode == Mode.Low || mode == Mode.LowPassword)
                {
                    fs.TryFlush();
                    fs.TryClose();
                    return Decode_Low(file, password);
                }

                if (mode != Mode.None && mode != Mode.NonePassword)
                    return false;

                fileName = data.GetStringDecoded(4);
                int fileNemeBaseLength = fileName.Length; //original name length must be set in order to calculate real offset
                if (mode == Mode.NonePassword)
                    fileName = AES256.Decrypt(fileName, password.Release());

                if (!Files.IsValidFilename(fileName))
                    return false;

                int dataOffset = (fileNemeBaseLength * sizeof(char)) + 8;
                byte[] cutoutData = data.SubArray(
                    dataOffset + 4,
                    Int32Ex.FromBytes(data, dataOffset));

                if (!fs.TryWrite(ref cutoutData, 0))
                    return false;

                fs.SetLength(fileSizeOriginal);
            }
            finally
            {
                fs.TryFlush();
                fs.TryClose();
            }

            if (!Files.TryMove(file, Files.GetDirectory(file) + "\\" + fileName))
                return false;
            else
                return true;
        }
    }
}
