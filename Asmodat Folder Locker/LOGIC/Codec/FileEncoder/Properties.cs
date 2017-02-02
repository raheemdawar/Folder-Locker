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
using System.Threading;
using System.Threading.Tasks;
using static Asmodat_File_Lock.Codec;

namespace Asmodat_File_Lock
{
    public partial class FileEncoder
    {
        private AES256 AES256 = new AES256();
        ThreadedMethod Methods;
        public CodecCounter Counter { get; private set; } = new CodecCounter();
        public bool IsBusy { get; private set; } = false;
        
        public string FileExtention { get; private set; }
        public int CutoutSizeMax { get; private set; } = 64;
        public int NewFileNameLengthMin { get; private set; } = 3;
        public int NewFileNameLengthMax { get; private set; } = 12;

        /// <summary>
        /// Seed size is used to to generate random hash array used to encrypt data content
        /// </summary>
        public int MaxSeedSize { get; private set; } = 128;

        private bool Stop { get; set; } = false;
        
        public int ThreadsCountMax { get { return this.Methods.MaxThreadsCount; } set { this.Methods.MaxThreadsCount = value; } }

        public FileEncoder(int maxThreadsCount, int MaxSeedSize, string FileExtention, int CutoutSizeMax, int NewFileNameLengthMin, int NewFileNameLengthMax)
        {
            this.Methods = new ThreadedMethod(maxThreadsCount);
            this.FileExtention = FileExtention;
            this.CutoutSizeMax = CutoutSizeMax;
            this.NewFileNameLengthMin = NewFileNameLengthMin;
            this.NewFileNameLengthMax = NewFileNameLengthMax;
            this.MaxSeedSize = MaxSeedSize;
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
            GeneratedFileNames.Clear();
            IsBusy = false;
        }
    }
}
