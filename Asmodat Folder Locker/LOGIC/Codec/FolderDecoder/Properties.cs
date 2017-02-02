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
using System.Threading;
using System.Threading.Tasks;

namespace Asmodat_File_Lock
{
    public partial class FolderDecoder
    {
        private AES256 AES256 = new AES256();
        ThreadedMethod Methods;
        public bool IsBusy { get; private set; } = false;
        public CodecCounter Counter { get; private set; } = new CodecCounter();
        public string FolderConfigFileName { get; private set; }

        public int ThreadsCountMax { get { return this.Methods.MaxThreadsCount; } set { this.Methods.MaxThreadsCount = value; } }

        public FolderDecoder(int maxThreadsCount, string FolderConfigFileName)
        {
            Methods = new ThreadedMethod(maxThreadsCount);
            this.FolderConfigFileName = FolderConfigFileName;
        }

        private bool Stop { get; set; } = false;

        private void Join()
        {
            while (this.IsBusy)
                Thread.Sleep(10);
        }

        public void Terminate()
        {
            Stop = true;
            this.Join();
            Stop = false;
            Counter.Reset();
            IsBusy = false;
        }
    }
}
