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
    public partial class CodecCounter
    {
        public int Uncompleated { get { return (Total - Success); } }
        public int Success { get; set; } = 0;
        public int Total { get; set; } = 0;
        public int Compleated { get; set; } = 0;

        /// <summary>
        /// Returns progress in percentages
        /// </summary>
        public double Progress { get { return Total == 0 ? 0 :  ((double)Compleated / (double)Total) *100; } }

        public void Reset()
        {
            this.Success = 0;
            this.Compleated = 0;
            this.Total = 0;
        }
    }
}
