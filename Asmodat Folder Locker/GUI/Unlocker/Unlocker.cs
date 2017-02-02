using Asmodat.Abbreviate;
using Asmodat.Cryptography;
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

namespace Asmodat_File_Lock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void ControlsSetup_UnlockerStart()
        {
            UnlockerStarted = true;
            UpdateFileList();

            TBtnUnlocker.Background = Brushes.LightPink;
            TBtnUnlocker.Content = "STOP DECRYPTION";
            ShowPassword = false;

            TBtnSelectFolder.IsEnabled = false;
            TBtnUpdate.IsEnabled = false;
            TCbxMaxSecurity.IsEnabled = false;
            TCbxIncludeFolderNames.IsEnabled = false;
            TCbxIncludeSubdirectories.IsEnabled = false;
            TCbxKillLockingProcesses.IsEnabled = false;
            TPTbxPassword.IsEnabled = false;
            TBtnLocker.IsEnabled = false;
            TBtnShowHidePassword.IsEnabled = false;
        }

        public void ControlsSetup_UnlockerStop()
        {
            TBtnSelectFolder.IsEnabled = true;
            TBtnUpdate.IsEnabled = true;
            TCbxMaxSecurity.IsEnabled = true;
            TCbxIncludeFolderNames.IsEnabled = true;
            TCbxIncludeSubdirectories.IsEnabled = true;
            TCbxKillLockingProcesses.IsEnabled = true;
            TPTbxPassword.IsEnabled = true;
            TBtnLocker.IsEnabled = true;
            TBtnShowHidePassword.IsEnabled = true;

            TBtnUnlocker.Content = "DECRYPT";
            TBtnUnlocker.SetBackgroundToDefault();
            AFLCodec.ResetCounters();

            UpdateFileList();
            UnlockerStarted = false;
        }



        public void Unlock()
        {
            ControlsSetup_UnlockerStart();

            AFLCodec.FileDecoder.Decode(FilesLocked.ToArray(), TPTbxPassword.SecurePassword, TCbxKillLockingProcesses.IsChecked.Value);

            if (TCbxIncludeFolderNames.IsChecked.Value)
                AFLCodec.FolderDecoder.Decode(FoldersAll.ToArray(), TPTbxPassword.SecurePassword, TCbxKillLockingProcesses.IsChecked.Value);

            TLPBrProgressFile.Text = "Decryption done.";
            ControlsSetup_UnlockerStop();
        }

        
        public bool UnlockerStarted { get; private set; } = false;

        private void OnUnlocker_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsStopping)
                return;

            if (!UnlockerStarted) //Lock
               Methods.Run(() => Unlock(), null, true, false);
            else //stop decrypting
                Methods.Run(() => UnlockerStop());
        }

        public void UnlockerStop()
        {
            IsStopping = true;
            try
            {
                TBtnUnlocker.Content = "Stopping, please wait...";
                TBtnUnlocker.Background = Brushes.LightYellow;
                AFLCodec.Terminate();
                Methods.Join(() => Unlock());
                ControlsSetup_UnlockerStop();
            }
            finally
            {
                IsStopping = false;
            }
        }
    }
}
