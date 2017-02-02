using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Debugging;
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
        private bool Decode_Low(string file, SecureString password = null)
        {
            if (this.Stop) return false;


            string fileTemp = this.GenerateNewFileName(file);

            if (!Files.TryCopy(file, fileTemp))
                return false;

            FileStream fs = null;
            string fileName = null;
            bool result = true;
            try
            {
                
            fs = FileInfoEx.TryOpen(fileTemp, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                if (fs == null)
                    throw new Exception("Decryption temporary stream is null.");

            
                long fileSize = fs.Length;//save original size of the file
                long fileSizeOriginal;//file size that final result will be cut into

                if (!Int64Ex.TryFromBytes(out fileSizeOriginal, fs.TryRead(0, 8)))
                    throw new Exception("Decryption failed, could not decode file size");

                long offset = Math.Max(fileSizeOriginal, 8); //offset cannot be smaller then 8 bytes
                if (!offset.InOpenInterval(0, fs.Length))
                    throw new Exception("Decryption failed, offset interval invalid.");

                var data = fs.TryRead(offset, (int)(fs.Length - offset));

                Mode mode = (Mode)Int32Ex.FromBytes(data);

                if (mode != Mode.Low && mode != Mode.LowPassword)
                    throw new Exception("Decryption failed, wrong mode");

                fileName = data.GetStringDecoded(4);
                int fileNemeBaseLength = fileName.Length; //original name length must be set in order to calculate real offset
                if (mode == Mode.LowPassword)
                    fileName = AES256.Decrypt(fileName, password.Release());
                else
                    fileName = fileName.UnGZip(Encoding.UTF32);

                if (!Files.IsValidFilename(fileName))
                    throw new Exception("Decryption failed, invalid filename");

                int dataOffset = (fileNemeBaseLength * sizeof(char)) + 8;//+4B from start + 4B from fileName encoded length
                byte[] seed = data.DecodeWithLength(dataOffset);

                if (seed.IsNullOrEmpty())
                    throw new Exception("Decryption failed, seed is missing. (1)");

                int seedInitialLength = seed.Length;
                if (mode == Mode.LowPassword)
                {
                    seed = AES256.Decrypt(seed, password.Release());
                    if (seed.IsNullOrEmpty())
                        throw new Exception("Decryption failed, seed is missing. (2)");
                }

                dataOffset += seedInitialLength + 4;// + 4B from fileName encoded length
                byte[] cutoutData = data.SubArray(
                    dataOffset + 4,
                    Int32Ex.FromBytes(data, dataOffset));

                if (!fs.TryWrite(ref cutoutData, 0))
                    throw new Exception("Decryption failed, could not write. (1)");

                fs.SetLength(fileSizeOriginal);

                if (!this.StreamDecoder_Low(fs, seed))
                    throw new Exception("Decryption failed, could not write. (2)");
            }
            catch(Exception ex)
            {
                ex.ToOutput();
                result = false;
            }
            finally
            {
                fs.TryFlush();
                fs.TryClose();
            }

            if(!result)
            {
                Files.Delete(fileTemp);
                return false;
            }
            else if (Files.TryMove(fileTemp, Files.GetDirectory(fileTemp) + "\\" + fileName))
                return Files.Delete(file);
            else
                return false;
        }
    }
}
