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
using System.Threading.Tasks;
using static Asmodat_File_Lock.Codec;

namespace Asmodat_File_Lock
{
    public partial class FileEncoder
    {
        /// <summary>
        /// This is my basic stream encoder method, that is using random noise seed, (noise seed should be of random length).
        /// Althow seed might not be as big as a whole strem, yeat new seeds are generaded in evry iteration based on encrypted data.
        /// FileDecoder class should contains a method called StreamDecoder_Low, that is able to recreate original stream.
        /// Obvoiusly seed must be protected with at least SHA256 while encoding files.
        /// </summary>
        /// <param name="stream">Stream to encode.</param>
        /// <param name="seed">Random seed of random length to encode data.</param>
        /// <returns>true if operation was successful else false</returns>
        private bool StreamEncoder_Low(FileStream streamR, FileStream streamW, byte[] seed)
        {
            try
            {
                if (streamR == null || streamW == null || !streamR.CanRead || !streamW.CanWrite)
                    return false;

                long length = streamR.Length;

                if (length == 0) //no need for encoding
                    return true;

                int bufforSize = seed.Length, i;
                byte[] buffor = new byte[bufforSize];

                streamR.Position = 0;
                streamW.Position = 0;

                while (streamR.Position < length)
                {
                    if (this.Stop) return false;


                    if (streamR.Position + bufforSize > length)
                        bufforSize = (int)(length - streamR.Position);

                    streamR.Read(buffor, 0, bufforSize);

                    buffor[0] ^= seed[0];
                    seed[0] = buffor[0];

                    //Parallel.For(1, bufforSize, idx => { buffor[idx] ^= seed[idx]; });
                    //Parallel.For(1, bufforSize, idx => { seed[idx] = (byte)(buffor[idx] ^ seed[idx - 1]); });

                    for (i = 1; i < bufforSize; i++)
                    {
                        buffor[i] ^= seed[i];
                        seed[i] = (byte)(buffor[i] ^ seed[i - 1]);
                    }

                    //stream.Position -= bufforSize;
                    streamW.Write(buffor, 0, bufforSize);
                }

                return true;
            }
            catch(Exception ex)
            {
                ex.ToOutput();
                return false;
            }
        }
    }
}


/* Some diffrent, working ideas:
 





public static bool StreamCodec_Low(FileStream stream, byte[] seed)
        {
            try
            {
                if (stream == null || !stream.CanRead || !stream.CanWrite || !stream.CanSeek)
                    return false;

                long length = stream.Length;
                int bufforSize = seed.Length, i;
                byte[] buffor = new byte[bufforSize];

                stream.Position = 0;
                while (stream.Position < length)
                {
                    if (stream.Position + bufforSize > length)
                        bufforSize = (int)(length - stream.Position);

                    stream.Read(buffor, 0, bufforSize);

                    for (i = 0; i < bufforSize; i++)
                        buffor[i] ^= seed[i];

                    stream.Position -= bufforSize;
                    stream.Write(buffor, 0, bufforSize);
                }

                return true;
            }
            catch(Exception ex)
            {
                ex.ToOutput();
                return false;
            }
        }
 * 
 private bool TryEncodingSequnceXOR(FileStream stream, byte initialValue)
        {
            if (stream == null || !stream.CanRead || !stream.CanWrite || !stream.CanSeek)
                return false;

            long length = stream.Length;
            int bufforSize = 1024, lastByte = initialValue;
            byte[] buffor = new byte[bufforSize];

            stream.Position = 0;
            while (stream.Position < length)
            {
                if (stream.Position + bufforSize > length)
                    bufforSize = (int)(length - stream.Position);

                stream.Read(buffor, 0, bufforSize);

                for (int i = 0; i < bufforSize; i++)
                {
                    buffor[i] = (byte)(buffor[i] ^ lastByte);
                    lastByte = buffor[i];
                }

                stream.Position -= bufforSize;
                stream.Write(buffor, 0, bufforSize);
            }

            return true;
        }
     
     */
