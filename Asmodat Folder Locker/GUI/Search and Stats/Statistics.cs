using Asmodat.Abbreviate;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
using Asmodat.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Asmodat_File_Lock
{
    public partial class MainWindow : Window
    {
        public void Peacemaker_Statistics()
        {
            if(AFLCodec.IsBusy)
            {
                TLblTotalFiles.Content = $"Total files: ?";
                TLblTotalEncryptedFiles.Content = $"Encrypted files: ?";
                TLblTotalFolders.Content = $"Total folders: ?";
                TLblTotalEncryptedFolders.Content = $"Encrypted folders: ?";
            }
            else
            {
                TLblTotalFiles.Content = $"Total files: {FilesAll.Count}";
                TLblTotalEncryptedFiles.Content = $"Encrypted files: {FilesLocked.Count}";
                TLblTotalFolders.Content = $"Total folders: {FoldersAll.Count}";
                TLblTotalEncryptedFolders.Content = $"Encrypted folders: {FoldersLocked.Count}";
            }

            if(AFLCodec.FileEncoder.IsBusy)
            {
                TLPBrProgressFile.Text = $"Encoding Files, Progress: {(int)AFLCodec.FileEncoder.Counter.Progress}%";
                TLPBrProgressFile.Value = AFLCodec.FileEncoder.Counter.Progress;
            }
            else if (AFLCodec.FileDecoder.IsBusy)
            {
                TLPBrProgressFile.Text = $"Decoding Files, Progress: {(int)AFLCodec.FileDecoder.Counter.Progress}%";
                TLPBrProgressFile.Value = AFLCodec.FileDecoder.Counter.Progress;
            }
            else if (AFLCodec.FolderEncoder.IsBusy)
            {
                TLPBrProgressFile.Text = $"Encoding Folders, Progress: {(int)AFLCodec.FolderEncoder.Counter.Progress}%";
                TLPBrProgressFile.Value = AFLCodec.FolderEncoder.Counter.Progress;
            }
            else if (AFLCodec.FolderDecoder.IsBusy)
            {
                TLPBrProgressFile.Text = $"Decoding Folders, Progress: {(int)AFLCodec.FolderDecoder.Counter.Progress}%";
                TLPBrProgressFile.Value = AFLCodec.FolderDecoder.Counter.Progress;
            }
            else
            {
                TLPBrProgressFile.Value = 0;
            }
        }
    }
}
