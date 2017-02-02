using Asmodat.Abbreviate;
using Asmodat.Cryptography;
using Asmodat.Extensions.Collections.Generic;
using Asmodat.Extensions.Objects;
using Asmodat.Extensions.Windows.Controls;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ThreadedMethod Methods = new ThreadedMethod();
        ThreadedTimers Timers = new ThreadedTimers();
        Codec AFLCodec = new Codec(FileLockExtention, FolderConfigFileName);
        

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            Methods.Run(() => Initialize());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AFLCodec.IsBusy)
            {
                var result = System.Windows.MessageBox.Show(
@"Codec is busy ! 
Your data might be permanenty destroyed if you exit right now.
Please consider waiting until Encryption/Decryption is finalized.
You can click STOP ENCRYPTION/DECRYPTION to speed-up this process.

Press 'No' if you want to stay, press 'Yes' if you want to exit anyway.", "STOP !!! Are you sure, you want to exit ?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    System.Windows.Application.Current.Shutdown();
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
        }

        private void Initialize()
        {
            TCbxIncludeSubdirectories.IsChecked = false;

            this.Path = Directories.Current;
            Timers.Run(() => Peacemaker_Statistics(), 250);
            Methods.Run(() => UpdateFileList(), null, 500, true, false);

            ShowPassword = false;
            TCbxIncludeSubdirectories.IsChecked = true;
        }
        
        private void OnTCbxIncludeSubdirectories_Click(object sender, RoutedEventArgs e)
        {
            Methods.Run(() => UpdateFileList());
        }

        private void OnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Methods.Run(() => UpdateFileList());
        }

        private bool _ShowPassword = true;
        public bool ShowPassword
        {
            get { return _ShowPassword; }
            set
            {
                this.TryInvokeMethodAction(() => {  

                TBtnShowHidePassword.ImageBrushSource = value ?
                    new BitmapImage(new Uri("pack://application:,,,/IMAGES/EyeClosed3.png")):
                    new BitmapImage(new Uri("pack://application:,,,/IMAGES/EyeOpen3.png"));

                TPTbxPassword.PasswordVisible = value;
                });
                _ShowPassword = value;
            }
        }
        private void OnTBtnShowHidePassword_Click(object sender, RoutedEventArgs e)
        {
            ShowPassword = !ShowPassword;
        }

        private void OnTBtnHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
@"1. Click 'SELECT FOLDER' and select folder that contains all files, that you wish to encrypt/decrypt

2. If you want to encrypt all files and folders that are contained inside selected folder then check the 'Include Subdirectories' checkbox.

3. After selecting 'Include Subdirectories' you can Encrypt / Decrypt folders names that contain your files by checking the 'Encrypt/Decrypt Folder Names' checkbox.

4. If you want to be sure, that all files that you have access to will be encrypted / decrypted (all programs that any of them might be opened, should  be closed automatically) then check 'Kill Locking Processes And Attributes'.

5. To prevent someone from Decrypting your files simply insert password that should be used for encryption. (You can preview your password by clicking opened eye icon, to be sure, that what you typed is correct.)

6. If you need to hide sensitive data and want it fully encrypted check 'Max Security' checkbox. It is also advised to set password in this mode. (This mode is perfect for small documents, but takes a long time to encrypt large files.)

7. Click 'ENCRYPT' if you want to encrypt your files or folders, click 'DECRYPT' if you want to decrypt your files or folders.

8. Profit ?


Remember this program is only a basic means of files encryption (Without Max Security mode checked), and is very good if time is of the essence. It will most certainly prevent ordinary person form opening your files or figuring what they are. 
After using password encryption all files and folder names will be hidden using very secure SHA256 algorithm, but only part of data that resides inside your 'encrypted' files will be encrypted, like file headers etc., thus making them unopenable by most software even if extension is changed to original one. The other part of your data is encrypted depending on level of privacy you choose, take this into account when selecting options.

It is very important that you don't close this application while it is Encrypting/Decrypting/Stopping Encryption/Decryption, otherwise your files might be permanently and irreversibly destroyed. 

If you have any great ideas, how to improve this program, you want me to make it even more secure, you found a bug, or you wish for a new feature, please send me an email: asmodat@gmail.com

License / Use conditions: I charge no money for my programs and forbid anyone to do so, but also take no responsibility if any of my programs harms / destroys your data or your computer in any way (or for example you forget your password and cannot access what you encrypted). You are not allowed to use my programs if you disagree with that."
);
        }
    }
}
