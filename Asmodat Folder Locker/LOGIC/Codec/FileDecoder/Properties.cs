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
using System.Threading;
using System.Threading.Tasks;
using static Asmodat_File_Lock.Codec;

namespace Asmodat_File_Lock
{
    public partial class FileDecoder
    {
        private AES256 AES256 = new AES256();

        ThreadedMethod Methods;
        public CodecCounter Counter { get; private set; } = new CodecCounter();
        public bool IsBusy { get; private set; } = false;
        
        private bool Stop { get; set; } = false;

        public int NewFileNameLengthMin { get; private set; } = 3;
        public int NewFileNameLengthMax { get; private set; } = 12;
        public string FileExtention { get; private set; }

        public int ThreadsCountMax { get { return this.Methods.MaxThreadsCount; } set { this.Methods.MaxThreadsCount = value; } }
        public FileDecoder(int maxThreadsCount, string FileExtention, int NewFileNameLengthMin, int NewFileNameLengthMax)
        {
            this.Methods = new ThreadedMethod(maxThreadsCount);
            this.FileExtention = FileExtention;
            this.NewFileNameLengthMin = NewFileNameLengthMin;
            this.NewFileNameLengthMax = NewFileNameLengthMax;
        }

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
