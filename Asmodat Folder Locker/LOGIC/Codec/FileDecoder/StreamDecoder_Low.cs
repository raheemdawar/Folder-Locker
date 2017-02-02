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
        private bool StreamDecoder_Low(FileStream stream, byte[] seed)
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
                byte save;

                stream.Position = 0;

                while (stream.Position < length)
                {
                    if (this.Stop) return false;

                    if (stream.Position + bufforSize > length)
                        bufforSize = (int)(length - stream.Position);

                    stream.Read(buffor, 0, bufforSize);

                    save = buffor[0];
                    buffor[0] ^= seed[0];
                    seed[0] = save;
                    for (i = 1; i < bufforSize; i++)
                    {
                        save = buffor[i];
                        buffor[i] ^= seed[i];
                        seed[i] = (byte)(save ^ seed[i - 1]);
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
        }


    }
}

/*
 private bool TryDecodingSequnceXOR(FileStream stream, byte initialValue)
        {
            if (stream == null || !stream.CanRead || !stream.CanWrite || !stream.CanSeek)
                return false;

            long length = stream.Length;
            int bufforSize = 1024, lastByte = initialValue, lastSave;
            byte[] buffor = new byte[bufforSize];

            stream.Position = 0;
            while (stream.Position < length)
            {
                if (stream.Position + bufforSize > length)
                    bufforSize = (int)(length - stream.Position);

                stream.Read(buffor, 0, bufforSize);

                for (int i = 0; i < bufforSize; i++)
                {
                    lastSave = buffor[i];
                    buffor[i] = (byte)(buffor[i] ^ lastByte);
                    lastByte = lastSave;
                }

                stream.Position -= bufforSize;
                stream.Write(buffor, 0, bufforSize);
            }

            return true;
        }
     

     */
