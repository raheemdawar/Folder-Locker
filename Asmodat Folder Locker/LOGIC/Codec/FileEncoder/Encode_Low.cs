using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Debugging;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.IO;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
using Asmodat.Types;
using AsmodatMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Asmodat_File_Lock.Codec;

namespace Asmodat_File_Lock
{
    public partial class FileEncoder
    {
        private bool Encode_Low(string file, SecureString password = null, bool killLockers = false)
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

            string fileTemp = this.GenerateNewFileName(file);

            List<byte> buff = new List<byte>();

            //save mode
            if (password == null)
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.Low)));
            else
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.LowPassword)));

            if (password != null) //encode filename with password
                buff.AddRange(AES256.Encrypt(Files.GetName(file, true), password.Release()).GetBytesEncoded());
            else //don't encode filename
                buff.AddRange((Files.GetName(file, true).GZip(Encoding.UTF32)).GetBytesEncoded());

            FileStream fs = null, fsTemp = null;
            bool result = true;
            try
            {
                fs = FileInfoEx.TryOpen(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fsTemp = FileInfoEx.TryOpen(fileTemp, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                if (fs == null || fsTemp == null)
                    throw new Exception("Stream or temporary stream invalid.");

                long fileSize = fs.Length;//save original size of the file

                //seed consists of random bytes, seed lenght should not be greater then file size, seed length will also be randomized
                //byte[] seed = AMath.RandomBytes((int)((fileSize <= 0 || fileSize > this.MaxSeedSize) ? AMath.Random(this.MaxSeedSize/2, this.MaxSeedSize+1) : fileSize));
                byte[] seed = AMath.RandomBytes((int)((this.MaxSeedSize > fileSize) ? Math.Max(fileSize / 2, 1) : AMath.Random(this.MaxSeedSize / 2, this.MaxSeedSize + 1)));

                if (password != null) //encode seed with password
                    buff.AddRange(AES256.Encrypt(seed, password.Release()).EncodeWithLength());
                else //don't encode seed
                    buff.AddRange(seed.EncodeWithLength());

                if (!this.StreamEncoder_Low(fs, fsTemp, seed))
                    throw new Exception("Stream encoding failed");

                int cutoutSize = (int)Math.Min(CutoutSizeMax, fileSize); //file size might be smaller then defined coutout

                buff.AddRange(Int32Ex.ToBytes(cutoutSize));
                buff.AddRange(fsTemp.TryRead(0, cutoutSize));

                byte[] dataNew = new byte[Math.Max(cutoutSize, 8)];
                Array.Copy(Int64Ex.ToBytes(fileSize), dataNew, 8);

                if (!fsTemp.TryWrite(ref dataNew, 0)) //write eof information to beggining of file
                    throw new Exception("Writing to encoded stream failed. (1)");

                if (!fsTemp.TryWrite(buff, fsTemp.Length)) //write data to the end of file
                    throw new Exception("Writing to encoded stream failed. (2)");
            }
            catch (Exception ex)
            {
                result = false;
                ex.ToOutput();
            }
            finally
            {
                fs.TryFlush();
                fs.TryClose();
                fsTemp.TryFlush();
                fsTemp.TryClose();
            }

            return result ? Files.Delete(file) : Files.Delete(fileTemp);
        }

        /*


        private bool Encode_Low(string file, SecureString password = null, bool killLockers = false)
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
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.Low)));
            else
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.LowPassword)));

            if (password != null) //encode filename with password
                buff.AddRange(AES256.Encrypt(Files.GetName(oldFile, true), password.Release()).GetBytesEncoded());
            else //don't encode filename
                buff.AddRange((Files.GetName(oldFile, true).GZip(Encoding.UTF32)).GetBytesEncoded());

            FileStream fs = FileInfoEx.TryOpen(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            if (fs == null)
                return false;

            try
            {
                long fileSize = fs.Length;//save original size of the file

                //seed consists of random bytes, seed lenght should not be greater then file size, seed length will also be randomized
                //byte[] seed = AMath.RandomBytes((int)((fileSize <= 0 || fileSize > this.MaxSeedSize) ? AMath.Random(this.MaxSeedSize/2, this.MaxSeedSize+1) : fileSize));
                byte[] seed = AMath.RandomBytes((int)((this.MaxSeedSize > fileSize) ? Math.Max(fileSize / 2, 1) : AMath.Random(this.MaxSeedSize / 2, this.MaxSeedSize + 1)));

                if (password != null) //encode seed with password
                    buff.AddRange(AES256.Encrypt(seed, password.Release()).EncodeWithLength());
                else //don't encode seed
                    buff.AddRange(seed.EncodeWithLength());

                if (!this.StreamEncoder_Low2(fs, seed))
                    return false;

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




        private bool StreamEncoder_Low2(FileStream stream, byte[] seed)
        {
            try
            {
                if (stream == null || !stream.CanRead || !stream.CanWrite || !stream.CanSeek)
                    return false;

                long length = stream.Length;

                if (length == 0) //no need for encoding
                    return true;

                int bufforSize = seed.Length, i;
                byte[] buffor = new byte[bufforSize];

                stream.Position = 0;

                while (stream.Position < length)
                {
                    if (stream.Position + bufforSize > length)
                        bufforSize = (int)(length - stream.Position);

                    stream.Read(buffor, 0, bufforSize);

                    buffor[0] ^= seed[0];
                    seed[0] = buffor[0];

                    //Parallel.For(1, bufforSize, idx => { buffor[idx] ^= seed[idx]; });
                    //Parallel.For(1, bufforSize, idx => { seed[idx] = (byte)(buffor[idx] ^ seed[idx - 1]); });

                    for (i = 1; i < bufforSize; i++)
                    {
                        buffor[i] ^= seed[i];
                        seed[i] = (byte)(buffor[i] ^ seed[i - 1]);
                    }

                    stream.Position -= bufforSize;
                    stream.Write(buffor, 0, bufforSize);
                }

                return true;
            }
            catch (Exception ex)
            {
                ex.ToOutput();
                return false;
            }
        }*/

    }
}






/*
 private bool Encode_Low(string file, SecureString password = null, bool killLockers = false)
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

            string fileTemp = this.GenerateNewFileName(file);

            List<byte> buff = new List<byte>();

            //save mode
            if (password == null)
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.Low)));
            else
                buff.AddRange(Int32Ex.ToBytes(((Int32)Mode.LowPassword)));

            if (password != null) //encode filename with password
                buff.AddRange(AES256.Encrypt(Files.GetName(file, true), password.Release()).GetBytesEncoded());
            else //don't encode filename
                buff.AddRange((Files.GetName(file, true).GZip(Encoding.UTF32)).GetBytesEncoded());

            FileStream fs = null, fsTemp = null;
            bool result = true;
            try
            {
                fs = FileInfoEx.TryOpen(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fsTemp = FileInfoEx.TryOpen(fileTemp, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                if (fs == null || fsTemp == null)
                    throw new Exception("Stream or temporary stream invalid.");

                long fileSize = fs.Length;//save original size of the file

                //seed consists of random bytes, seed lenght should not be greater then file size, seed length will also be randomized
                //byte[] seed = AMath.RandomBytes((int)((fileSize <= 0 || fileSize > this.MaxSeedSize) ? AMath.Random(this.MaxSeedSize/2, this.MaxSeedSize+1) : fileSize));
                byte[] seed = AMath.RandomBytes((int)((this.MaxSeedSize > fileSize) ? Math.Max(fileSize / 2, 1) : AMath.Random(this.MaxSeedSize / 2, this.MaxSeedSize + 1)));

                if (password != null) //encode seed with password
                    buff.AddRange(AES256.Encrypt(seed, password.Release()).EncodeWithLength());
                else //don't encode seed
                    buff.AddRange(seed.EncodeWithLength());

                if (!this.StreamEncoder_Low(fs, fsTemp, seed))
                    throw new Exception("Stream encoding failed");

                int cutoutSize = (int)Math.Min(CutoutSizeMax, fileSize); //file size might be smaller then defined coutout

                buff.AddRange(Int32Ex.ToBytes(cutoutSize));
                buff.AddRange(fs.TryRead(0, cutoutSize));

                byte[] dataNew = new byte[Math.Max(cutoutSize, 8)];
                Array.Copy(Int64Ex.ToBytes(fileSize), dataNew, 8);

                if (!fsTemp.TryWrite(ref dataNew, 0)) //write eof information to beggining of file
                    throw new Exception("Writing to encoded stream failed. (1)");

                if (!fsTemp.TryWrite(buff, fsTemp.Length)) //write data to the end of file
                    throw new Exception("Writing to encoded stream failed. (2)");
            }
            catch (Exception ex)
            {
                result = false;
                ex.ToOutput();
            }
            finally
            {
                fs.TryFlush();
                fs.TryClose();
                fsTemp.TryFlush();
                fsTemp.TryClose();
            }

            return result ? Files.Delete(file) : Files.Delete(fileTemp);
        }
     
     
     */


