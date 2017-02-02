using Asmodat.Abbreviate;
using Asmodat.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Asmodat.Cryptography;
using Asmodat.Extensions.Objects;
using System.Security;

namespace Asmodat_File_Lock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void ControlsSetup_LockerStart()
        {
            LockerStarted = true;
            UpdateFileList();

            TBtnLocker.Background = Brushes.LightPink;
            TBtnLocker.Content = "STOP ENCRYPTION";
            ShowPassword = false;

            TBtnSelectFolder.IsEnabled = false;
            TBtnUpdate.IsEnabled = false;
            TCbxMaxSecurity.IsEnabled = false;
            TCbxIncludeFolderNames.IsEnabled = false;
            TCbxIncludeSubdirectories.IsEnabled = false;
            TCbxKillLockingProcesses.IsEnabled = false;
            TPTbxPassword.IsEnabled = false;
            TBtnUnlocker.IsEnabled = false;
            TBtnShowHidePassword.IsEnabled = false;
        }

        public void ControlsSetup_LockerStop()
        {
            TBtnSelectFolder.IsEnabled = true;
            TBtnUpdate.IsEnabled = true;
            TCbxMaxSecurity.IsEnabled = true;
            TCbxIncludeFolderNames.IsEnabled = true;
            TCbxIncludeSubdirectories.IsEnabled = true;
            TCbxKillLockingProcesses.IsEnabled = true;
            TPTbxPassword.IsEnabled = true;
            TBtnUnlocker.IsEnabled = true;
            TBtnShowHidePassword.IsEnabled = true;

            TBtnLocker.Content = "ENCRYPT";
            TBtnLocker.SetBackgroundToDefault();
            AFLCodec.ResetCounters();

            UpdateFileList();
            LockerStarted = false;
        }

        public void Lock()
        {
            ControlsSetup_LockerStart();

            Codec.Mode mode = TCbxMaxSecurity.IsChecked.Value ?
                (!TPTbxPassword.SecurePassword.IsNullOrEmpty() ? Codec.Mode.LowPassword : Codec.Mode.Low) :
                (!TPTbxPassword.SecurePassword.IsNullOrEmpty() ? Codec.Mode.NonePassword : Codec.Mode.None);

            AFLCodec.FileEncoder.Encode(FilesUnlocked.ToArray(), mode, TPTbxPassword.SecurePassword, TCbxKillLockingProcesses.IsChecked.Value);

            if (TCbxIncludeFolderNames.IsChecked.Value)
                AFLCodec.FolderEncoder.Encode(FoldersAll.ToArray(), TPTbxPassword.SecurePassword, TCbxKillLockingProcesses.IsChecked.Value);

            TLPBrProgressFile.Text = "Encryption done.";

            ControlsSetup_LockerStop();
        }

        public bool LockerStarted { get; private set; } = false;

        private void OnLocker_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsStopping)
                return;

            if (!LockerStarted) //Lock
                Methods.Run(() => Lock(), null, true, false);
            else //stop encrypting
                Methods.Run(() => LockerStop());
        }
        /*
         Methods.Terminate(() => Lock());
                Methods.Run(() => AFLCodec.Terminate());
                ControlsSetup_LockerStop();
             */


        public bool IsStopping { get; private set; } = false;
        public void LockerStop()
        {
            IsStopping = true;
            try
            {
                TBtnLocker.Content = "Stopping, please wait...";
                TBtnLocker.Background = Brushes.LightYellow;
                AFLCodec.Terminate();
                Methods.Join(() => Lock());
                ControlsSetup_LockerStop();
            }
            finally
            {
                IsStopping = false;
            }
        }

    }
}
