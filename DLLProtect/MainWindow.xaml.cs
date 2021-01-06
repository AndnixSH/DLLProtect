using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;

namespace DLLProtect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog sfile = new OpenFileDialog();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void protectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.backupFileChkBox.IsChecked == true)
                {
                    File.Copy(pathTextBox.Text, Path.GetDirectoryName(pathTextBox.Text) + "\\" + Path.GetFileNameWithoutExtension(pathTextBox.Text) + "_bak_unprotected.dll", true);
                }
                Stream st = File.Open(pathTextBox.Text, FileMode.Open);
                st.Seek(0x83, SeekOrigin.Begin);
                st.WriteByte(0x01);

                st.Seek(0x86, SeekOrigin.Begin);
                st.WriteByte(0x04);

                st.Seek(0xf4, SeekOrigin.Begin);
                st.WriteByte(0x08);

                st.Seek(0x109, SeekOrigin.Begin);
                st.WriteByte(0xe0);
                st.WriteByte(0x71);

                //don't know if it works
                //st.Seek(0x18d, SeekOrigin.Begin);
                //st.WriteByte(0x04);

                st.Seek(0x1d9, SeekOrigin.Begin);
                st.WriteByte(0x04);

                st.Seek(0x1ad, SeekOrigin.Begin);
                st.WriteByte(0x88);
                st.WriteByte(0x71);

                st.Seek(0x1b5, SeekOrigin.Begin);
                st.WriteByte(0xb4);
                st.WriteByte(0x71);
                st.Close();

                protectBtn.IsEnabled = false;
                unprotectBtn.IsEnabled = true;
                checkIfProtectedLbl.Content = "DLL is protected";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "DLLProtect", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void unprotectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullpath = pathTextBox.Text;
                if (this.backupFileChkBox.IsChecked == true)
                {
                    File.Copy(pathTextBox.Text, Path.GetDirectoryName(pathTextBox.Text) + "\\" + Path.GetFileNameWithoutExtension(pathTextBox.Text) + "_bak_protected.dll", true);
                }
                Stream st = File.Open(pathTextBox.Text, FileMode.Open);
                st.Seek(0x83, SeekOrigin.Begin);
                st.WriteByte(0x00);

                st.Seek(0x86, SeekOrigin.Begin);
                st.WriteByte(0x03);

                st.Seek(0xf4, SeekOrigin.Begin);
                st.WriteByte(0x10);

                st.Seek(0x109, SeekOrigin.Begin);
                st.WriteByte(0x40);
                st.WriteByte(0x46);

                st.Seek(0x18d, SeekOrigin.Begin);
                st.WriteByte(0x02);

                st.Seek(0x1ad, SeekOrigin.Begin);
                st.WriteByte(0x40);
                st.WriteByte(0x46);

                st.Seek(0x1b5, SeekOrigin.Begin);
                st.WriteByte(0x18);
                st.WriteByte(0x46);
                st.Close();
                protectBtn.IsEnabled = true;
                unprotectBtn.IsEnabled = false;
                checkIfProtectedLbl.Content = "DLL is unprotected";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "DLLProtect", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void selectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            sfile.Filter = "DLL file (*.dll)|*.dll";
            sfile.Title = "Select the DLL file to protect/unprotect";
            sfile.ShowDialog(); //Show dialog
            pathTextBox.Text = sfile.FileName;
            protectBtn.IsEnabled = true;
            unprotectBtn.IsEnabled = false;
            binreader();
        }

        private void binreader()
        {
            try
            {
                BinaryReader br = new BinaryReader(File.OpenRead(pathTextBox.Text));
                string pted = null;
                br.BaseStream.Position = 0x83;
                pted += br.ReadByte().ToString("X2");
                if (pted == "01")
                {
                    protectBtn.IsEnabled = false;
                    unprotectBtn.IsEnabled = true;
                    checkIfProtectedLbl.Visibility = Visibility.Visible;
                    checkIfProtectedLbl.Content = "DLL is protected";
                }
                else
                {
                    protectBtn.IsEnabled = true;
                    unprotectBtn.IsEnabled = false;
                    checkIfProtectedLbl.Visibility = Visibility.Visible;
                    checkIfProtectedLbl.Content = "DLL is unprotected";
                }
                br.Close();
                br.Dispose();
            }
            catch
            {

            }
        }

        #region drag drop
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (ext.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            grid.Background = Brushes.White;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (ext.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    grid.Background = Brushes.LightSkyBlue;
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            protectBtn.IsEnabled = true;
            unprotectBtn.IsEnabled = true;
            grid.Background = Brushes.White;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (ext.Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                {
                    pathTextBox.Text = file;
                    binreader();
                }
            }
        }

        private void win_Closed(object sender, EventArgs e)
        {
            RegistryKey regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DLLProtect");
            regKey.SetValue("backupFileChkBox", backupFileChkBox.IsChecked, RegistryValueKind.DWord);
            regKey.Close();
        }
        #endregion
    }
}
