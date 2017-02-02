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
        /// <summary>
        /// Describes visual bechavior of controls during update
        /// </summary>
        public void ControlsSetup_SearchStart()
        {
            TBtnSelectFolder.Content = "STOP SEARCH";
            TBtnSelectFolder.Background = Brushes.LightPink;
            TCbxIncludeSubdirectories.IsEnabled = false;
            TBtnUpdate.IsEnabled = false;
            TPTbxPassword.IsEnabled = false;
            TBtnLocker.IsEnabled = false;
            TBtnUnlocker.IsEnabled = false;
        }

        /// <summary>
        /// Describes visual bechavior of controls after update
        /// </summary>
        public void ControlsSetup_SearchEnd()
        {
            TBtnSelectFolder.Content = "SELECT FOLDER";
            TBtnSelectFolder.SetBackgroundToDefault();
            TCbxIncludeSubdirectories.IsEnabled = true;
            TBtnUpdate.IsEnabled = true;
            TPTbxPassword.IsEnabled = true;// TCbxEnablePassword.IsChecked.Value;
            TBtnLocker.IsEnabled = true;
            TBtnUnlocker.IsEnabled = true;
        }


        public bool IsUpdatingFileList { get { return Methods.IsAlive(() => UpdateFileList()); } }

        public void FileListClear()
        {
            Methods.Terminate(() => UpdateFileList());
            FilesAll.Clear();
            FoldersAll.Clear();
            FilesLocked.Clear();
            FoldersLocked.Clear();
            FoldersUnlocked.Clear();
            FilesUnlocked.Clear();
            ControlsSetup_SearchEnd();
        }


        public void UpdateFileList()
        {
            ControlsSetup_SearchStart();
            FilesAll.Clear();
            FoldersAll.Clear();

            var all = Directories.TryGetFilesAndDirectories(
                FilesAll, 
                new ThreadedList<string>() { this.Path }, 
                "*", 
                "*",
                TCbxIncludeSubdirectories.IsChecked.Value, 
                FoldersAll);

            var locked = all.Item1?.Where(s => s.EndsWith(FileLockExtention))?.ToList();
            var unlocked = all.Item1?.Where(s => locked?.Contains(s) == false && !s.EndsWithAny(ExeFullName, FolderConfigFileName))?.ToList();
            var dirs = all.Item2?.Where(s => s != null && (!s.StartsWith(ExeDirectory) || s.Length > ExeDirectory.Length))?.ToList();

            if (dirs?.Contains(this.Path) == true)
                dirs.Remove(this.Path);

            FilesAll.SetRange(locked, unlocked);
            FoldersAll.SetRange(dirs);

            this.FilesLocked = locked.IsNullOrEmpty() ? new List<string>() : locked.ToList();
            this.FilesUnlocked = unlocked.IsNullOrEmpty() ? new List<string>() : unlocked;

            var lockedFolders = FoldersAll?.Where(s => AFLCodec.FolderEncoder.IsFolderEncoded(s))?.ToList();
            var unlockedFolders = lockedFolders.IsNullOrEmpty() ? new List<string>() : FoldersAll?.Where(s => !lockedFolders.Contains(s))?.ToList();


            this.FoldersLocked = lockedFolders.IsNullOrEmpty() ? new List<string>() : lockedFolders;
            this.FoldersUnlocked = unlockedFolders.IsNullOrEmpty() ? new List<string>() : unlockedFolders;


            ControlsSetup_SearchEnd();
        }
    }
}
