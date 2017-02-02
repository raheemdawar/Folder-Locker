using Asmodat.Abbreviate;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.Objects;
using Asmodat.IO;
using Asmodat.Types;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        public static string ExeFullName = Files.ExeFullName;
        public static string ExeDirectory = Files.ExeDirectory;
        public const string FileLockExtention = ".aef";
        public const string FolderConfigFileName = "FolderConfig.afc";

        public List<string> FilesLocked = new List<string>();
        public List<string> FilesUnlocked = new List<string>();
        public List<string> FoldersLocked = new List<string>();
        public List<string> FoldersUnlocked = new List<string>();

        private ThreadedList<string> FilesAll = new ThreadedList<string>();
        private ThreadedList<string> FoldersAll = new ThreadedList<string>();

        private string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                TTbxFolderName.Text = _Path;
            }
        }
        

        private void OnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            if (IsUpdatingFileList)
            {//terminate
                this.FileListClear();
            }
            else
            {//select & update
                
                CommonOpenFileDialog cofd = new CommonOpenFileDialog();
                cofd.InitialDirectory = Directory.Exists(this.Path) ? this.Path : ExeDirectory;
                cofd.IsFolderPicker = true;
                cofd.Multiselect = false;

                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                    this.Path = cofd.FileNames.First();

                Methods.Run(() => UpdateFileList());
            }
        }

        private void OnFolderName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._Path == TTbxFolderName.Text) return;

            string path = TTbxFolderName.Text;

            if (path.IsNullOrEmpty())
                Path = Directories.Current;
            else
                Path = path;
        }
    }
}
