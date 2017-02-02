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
        public void Encode(string[] files, Mode mode, SecureString password = null, bool killLockers = false)
        {
            if (files.IsNullOrEmpty())
                return;

            Methods.JoinAll();

            IsBusy = true;
            Counter.Reset();
            Counter.Total = files.Length;
            GeneratedFileNames.Clear();

            if (ThreadsCountMax <= 0)
                Parallel.For(0, files.Length, index => { Encode(files[index], mode, password, true, killLockers); });
            else
                foreach (string file in files) Methods.Run(() => Encode(file, mode, password, true, killLockers), file, true, true);

            Methods.JoinAll();
            IsBusy = false;
        }


        public void Encode(string file, Mode mode, SecureString password = null, bool counter = false, bool killLockers = false)
        {
            switch (mode)
            {
                case Mode.None:
                    Counter.Success += Encode_None(file, null, killLockers) && counter ? 1 : 0;
                    break;
                case Mode.NonePassword:
                    Counter.Success += Encode_None(file, password, killLockers) && counter ? 1 : 0;
                    break;
                case Mode.Low:
                    Counter.Success += Encode_Low(file, null, killLockers) && counter ? 1 : 0;
                    break;
                case Mode.LowPassword:
                    Counter.Success += Encode_Low(file, password, killLockers) && counter ? 1 : 0;
                    break;
                default: throw new Exception("Encoding mode not supported.");
            }

            if (counter)
                ++Counter.Compleated;
        }
    }
}
