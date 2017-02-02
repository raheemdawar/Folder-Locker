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
        public void Decode(string[] files, SecureString password = null, bool killLockers = false)
        {
            if (files.IsNullOrEmpty())
                return;

            Methods.JoinAll();
            IsBusy = true;
            GeneratedFileNames.Clear();

            try
            {
                Counter.Reset();
                Counter.Total = files.Length;

                if (ThreadsCountMax <= 0)
                    Parallel.For(0, files.Length, index => { Decode(files[index], password, true, killLockers); });
                else
                    foreach (string file in files) Methods.Run(() => Decode(file, password, true, killLockers), file, true, true);

                Methods.JoinAll();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Decode(string file, SecureString password = null, bool counter = false, bool killLockers = false)
        {
            Counter.Success += Decode_All(file, password, killLockers) && counter ? 1 : 0;
            if (counter)
                ++Counter.Compleated;
        }
    }
}
