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

namespace Asmodat_File_Lock
{
    public partial class Codec
    {
        AES256 AES256 = new AES256();

        public FileEncoder FileEncoder { get; private set; }
        public FileDecoder FileDecoder { get; private set; }
        public FolderEncoder FolderEncoder { get; private set; }
        public FolderDecoder FolderDecoder { get; private set; }

        public string FileExtention { get; private set; }

        public Codec(string FileExtention, string FolderConfigFileName)
        {
            this.FileExtention = FileExtention;
            FileEncoder = new FileEncoder(0,1024*1024*8,this.FileExtention, 64, 3, 12);
            FileDecoder = new FileDecoder(0, this.FileExtention, 3, 12);
            FolderEncoder = new FolderEncoder(32, FolderConfigFileName, 3, 12);
            FolderDecoder = new FolderDecoder(32, FolderConfigFileName);
        }

        public enum Mode
        {
            None = 0x1,
            NonePassword = 0x2,
            Low = 0x4,
            LowPassword = 0x8
        }

        public bool IsBusy { get { return FolderDecoder.IsBusy || FolderEncoder.IsBusy || FileEncoder.IsBusy || FileDecoder.IsBusy; } }

        public void ResetCounters()
        {
            FileEncoder.Counter.Reset();
            FileDecoder.Counter.Reset();
            FolderEncoder.Counter.Reset();
            FolderDecoder.Counter.Reset();
        }

        public void Terminate()
        {
            FileEncoder.Terminate();
            FileDecoder.Terminate();
            FolderEncoder.Terminate();
            FolderDecoder.Terminate();
        }
    }
}
