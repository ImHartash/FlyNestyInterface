using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using Microsoft.Win32;

namespace FlyNestyInterface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String scriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserScripts");

        public MainWindow()
        {
            InitializeComponent();
            updateScripts();
        }

        private void showOverlay()
        {
            this.waitOverlay.Visibility = Visibility.Visible;
        }

        private void hideOverlay()
        {
            this.waitOverlay.Visibility = Visibility.Collapsed;
        }

        private void updateScripts()
        {
            if (!Directory.Exists(scriptsFolder))
            {
                Directory.CreateDirectory(scriptsFolder);
            }

            filesList.Items.Clear();
            var files = Directory.GetFiles(scriptsFolder, "*.*")
                .Where(f => f.EndsWith(".txt") || f.EndsWith(".lua"))
                .Select(Path.GetFileName);
            foreach (var file in files.ToArray())
            {
                filesList.Items.Add(file);
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void waitOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private async Task<bool> injectExecutor()
        {
            bool result = false;

            try
            {
                result = await Task.Run(() => API.execInject());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        } 

        private async Task<bool> executeExecutor()
        {
            String luaCode = new TextRange(this.scriptTextBox.Document.ContentStart, this.scriptTextBox.Document.ContentEnd).Text;
            bool result = false;

            try
            {
                result = await Task.Run(() => API.execExecuteLuaScript(luaCode));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }

        private async void execute_Click(object sender, RoutedEventArgs e)
        {
            showOverlay();
            await Task.Run(() => executeExecutor());
            hideOverlay();
        }

        private async void inject_Click(object sender, RoutedEventArgs e)
        {
            showOverlay();
            await Task.Run(() => injectExecutor());
            hideOverlay();
        }

        private void filesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filesList.SelectedItem != null)
            {
                String filePath = Path.Combine(scriptsFolder, filesList.SelectedItem.ToString());
                try
                {
                    this.scriptTextBox.Document.Blocks.Clear();
                    this.scriptTextBox.Document.Blocks.Add(new Paragraph(new Run(File.ReadAllText(filePath))));
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Internal error while reading file: {ex.Message}");
                }
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = scriptsFolder,
                Filter = "Text Files (*.txt)|*.txt|Lua Files (*.lua)|*.lua|All Files (*.*)|*.*"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, new TextRange(this.scriptTextBox.Document.ContentStart, this.scriptTextBox.Document.ContentEnd).Text);
                    updateScripts();
                } 
                catch(Exception ex)
                {
                    MessageBox.Show($"Internal error while saving file: {ex.Message}");
                }
            }
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            this.scriptTextBox.Document.Blocks.Clear();
        }

        private void reload_Click(object sender, RoutedEventArgs e)
        {
            updateScripts();
        }
    }
}
