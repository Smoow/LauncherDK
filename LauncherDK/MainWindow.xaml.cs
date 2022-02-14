using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

namespace LauncherDK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isUpdating = false;
        bool isOnline = false;

        public MainWindow()
        {
            InitializeComponent();
            updateBar.Minimum = 0;
            jogarBtn.IsEnabled = false;
            GetUpdateFiles();
            getNotices();
            Task.Run(() => checkInjector());
        }

        private void Open_Game(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory();

            try
            {
                Process.Start(System.IO.Path.Combine(path, "DK.exe"));
            }
            catch (Exception)
            {
                MessageBox.Show("Não foi possível encontrar DK.exe" + Environment.NewLine + "Verifique se o Launcher está na pasta do jogo.", "Erro ao abrir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MakeUpdate(object sender, RoutedEventArgs e)
        {
            // Para atualizar, o jogo precisa estar fechado.
            Process[] WYDProcess = Process.GetProcessesByName("DK");
            if (WYDProcess.Length > 0)
            {
                // Para encerrar o processo, caso necessario
                //WYDProcess[0].CloseMainWindow();
                MessageBox.Show("Feche o jogo antes de atualizá-lo.", "Erro ao atualizar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            attButton.IsEnabled = false;
            isUpdating = true;

            //updatingText.Content = "Atualizando...";
            if (!File.Exists("./prefile.dk"))
            {
                await MakeUpdateAll();
            }
            else
            {
                string tempPath = System.IO.Path.GetTempPath();
                string[] lines = File.ReadAllLines(tempPath + "updatesOnly.txt");
                string[] updateLocal = File.ReadAllLines("./prefile.dk");
                int lenU = updateLocal.Length;
                int len = lines.Length;
                string lineUpdateLocal = updateLocal[lenU - 1];
                string decryptedString = Cryptor.Decrypt(lineUpdateLocal);
                double maxValueByLine = Math.Ceiling(100.0 / lines.Length);
                int flag = 0;

                foreach (var line in lines)
                {
                    if (!line.Equals(decryptedString) && flag == 0)
                    {
                        updateBar.Value += maxValueByLine;
                        continue;
                    }
                    flag = 1;

                    string[] lineSplit = line.Split(' ');
                    updatingText.Content = "Baixando: " + lineSplit[1];
                    updateBar.Value = 0;
                    var client = new WebClient();
                    client.DownloadProgressChanged += Client_DownloadProgressChanged_UpdateBar;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted_UpdateBar;
                    await client.DownloadFileTaskAsync("https://portaldk.com.br/Launcher/" + lineSplit[1], tempPath + lineSplit[1]);

                    bool flagLauncherFile = false;
                    using (ZipArchive archive = ZipFile.OpenRead(tempPath + lineSplit[1]))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            // AutoUpdate Launcher Function...
                            if (entry.Name == ("Play DK.exe"))
                            {
                                if (File.Exists("./Play DK.exe"))
                                {
                                    flagLauncherFile = true;
                                    File.Move("./Play DK.exe", "./Play DK_OLD.exe");

                                    // Guardando a versao do update na prefile.dk
                                    StreamWriter filec = new StreamWriter("./prefile.dk");
                                    string encryptedstring = Cryptor.Encrypt(line);
                                    await filec.WriteLineAsync(encryptedstring);
                                    filec.Close();
                                }
                            }
                        }

                        //...continuação
                        if (flagLauncherFile)
                        {
                            //archive.ExtractToDirectory(tempPath);
                            ZipArchiveExtensions.ExtractToDirectory(archive, tempPath, true);
                            MessageBox.Show("Launcher Atualizado, por favor abra novamente", "Sucesso ao atualizar o Launcher", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (File.Exists("../Play DK_OLD.exe")) File.Delete("../Play DK_OLD.exe");
                            if (File.Exists("./Play DK_OLD.exe")) File.Delete("./Play DK_OLD.exe");

                            File.Move(System.IO.Path.Combine(tempPath, "Play DK.exe"), "./Play DK.exe");

                            isUpdating = false;
                            Application.Current.Shutdown();
                        }

                        //archive.ExtractToDirectory("./");
                        ZipArchiveExtensions.ExtractToDirectory(archive, "./", true);
                    }

                    File.Delete(tempPath + lineSplit[1]);
                    updateBar.Value += maxValueByLine;
                }

                await WriteNewVersionFile(tempPath);
                await VerifyUpdate();
                isUpdating = false;
            }
        }

        private async Task MakeUpdateAll()
        {
            string tempPath = System.IO.Path.GetTempPath();
            string[] lines = File.ReadAllLines(tempPath + "updatesOnly.txt");
            int len = lines.Length;
            double maxValueByLine = Math.Ceiling(100.0 / lines.Length);

            foreach (var line in lines)
            {
                string[] lineSplit = line.Split(' ');
                updatingText.Content = "Baixando: " + lineSplit[1];
                updateBar.Value = 0;
                var client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged_UpdateBar;
                client.DownloadFileCompleted += Client_DownloadFileCompleted_UpdateBar;
                await client.DownloadFileTaskAsync("https://portaldk.com.br/Launcher/" + lineSplit[1], tempPath + lineSplit[1]);

                bool flagLauncherFile = false;
                using (ZipArchive archive = ZipFile.OpenRead(tempPath + lineSplit[1]))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // AutoUpdate Launcher Function...
                        if (entry.Name == ("Play DK.exe"))
                        {
                            if (File.Exists("./Play DK.exe"))
                            {
                                flagLauncherFile = true;
                                File.Move("./Play DK.exe", "/Play DK_OLD.exe");
                                await WriteNewVersionFile(tempPath);
                            }
                        }
                    }

                    // ...continuação
                    if (flagLauncherFile)
                    {
                        //archive.ExtractToDirectory(tempPath);
                        ZipArchiveExtensions.ExtractToDirectory(archive, tempPath, true);
                        MessageBox.Show("Launcher Atualizado, por favor abra novamente", "Sucesso ao atualizar o Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                        isUpdating = false;
                        File.Delete("./Play DK_OLD.exe");
                        File.Move(tempPath + "Play DK.exe", "./Play DK.exe");
                        Application.Current.Shutdown();
                    }

                    //archive.ExtractToDirectory("./");
                    ZipArchiveExtensions.ExtractToDirectory(archive, "./", true);
                }

                File.Delete(tempPath + lineSplit[1]);
                updateBar.Value += maxValueByLine;
            }

            await WriteNewVersionFile(tempPath);
            await VerifyUpdate();
            isUpdating = false;
        }

        private static async Task WriteNewVersionFile(string tempPath)
        {
            StreamWriter filec = new StreamWriter("./prefile.dk");
            string[] lines = File.ReadAllLines(tempPath + "updatesOnly.txt");

            int len = lines.Length;
            string encryptedstring = Cryptor.Encrypt(lines[len - 1]);

            await filec.WriteLineAsync(encryptedstring);
            filec.Close();
        }

        private void Client_DownloadProgressChanged_UpdateBar(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            string[] tempString = updatingText.Content.ToString().Split('.');

            if (percentage >= 99.00)
            {
                updatingText.Content = "";
                updatingText.Content = tempString[0].Replace("Baixando", "Extraindo") + ".zip - Isso pode levar alguns segundos";
            }
            else
            {
                updatingText.Content = "";
                updatingText.Content = tempString[0] + ".zip - " + Math.Round(percentage, 2) + "%";
                updateBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            }
        }

        private void Client_DownloadFileCompleted_UpdateBar(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string[] tempString = updatingText.Content.ToString().Split('.');
        }

        private void cfgButton_Click(object sender, RoutedEventArgs e)
        {
            // Para configurar, o jogo precisa estar fechado.
            Process[] WYDProcess = Process.GetProcessesByName("DK");
            if (WYDProcess.Length > 0)
            {
                // Para encerrar o processo, caso necessario
                //WYDProcess[0].CloseMainWindow();
                MessageBox.Show("Feche o jogo antes de alterar sua configuração.", "Erro ao abrir configurações", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Config config = new Config();
            config.Show();
        }

        private async void getNotices()
        {
            const string URL = "https://portaldk.com.br/api/noticias";
            string urlParameters = "?token=JzdWIiOiIx-MjM0NTY3OD-kwIiwibmFt-ZSI6IkpvaG-4gRG9lIiwi-aWF0IjoxNT-E2MjM5MDIy";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // "Blocking call" => Program will wait here until a response is received or a timeout occurs.
            if (response.IsSuccessStatusCode)
            {
                // Cleaning TextBox
                not1.Text = "";


                // Parse the response body.
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<Notice>>().Result;  // We also need make sure to add a reference to System.Net.Http.Formatting.dll
                foreach (var d in dataObjects)
                {
                    not1.Text += $"{d.Dh_Inicial.ToString("dd/MM/yyyy")}\n{d.Ds_Tipo}: {d.Titulo}\n\n";
                }
            }
            else
            {
                // For debug purposes
                // Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            client.Dispose();
        }

        private async void GetUpdateFiles()
        {
            try
            {
                string tempPath = System.IO.Path.GetTempPath();
                var client = new WebClient();
                client.Headers.Add("User-Agent", "C# console program");

                Uri dk = new Uri("https://portaldk.com.br/Launcher/update1.htm");
                string content = client.DownloadString("https://portaldk.com.br/Launcher/update1.htm");
                StreamWriter file = new StreamWriter(tempPath + "tempfile.txt");
                await file.WriteLineAsync(content);
                file.Close();
                await WriteUpdateTextFile();
            }
            catch (Exception)
            {

            }
        }

        private async Task WriteUpdateTextFile()
        {
            string tempPath = System.IO.Path.GetTempPath();
            string[] lines = File.ReadAllLines(tempPath + "tempfile.txt");
             StreamWriter file = new StreamWriter(tempPath + "updatesOnly.txt");
            foreach (var line in lines)
            {
                if (line.Contains("Update")) await file.WriteLineAsync(line);
            }
            File.Delete(tempPath + "tempfile.txt");
            file.Close();
            await MakeVersionFile();
        }

        private async Task MakeVersionFile()
        {
            try
            {
                string tempPath = System.IO.Path.GetTempPath();
                if (!File.Exists("./prefile.dk"))
                {
                    attButton.IsEnabled = true;
                    updateBar.Value = 0;
                    updatingText.Content = "Nova versão disponível!";
                }
                else
                {
                    await VerifyUpdate();
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task VerifyUpdate()
        {
            string tempPath = System.IO.Path.GetTempPath();
            string[] lines = File.ReadAllLines(tempPath + "updatesOnly.txt");
            string[] updateLocal = File.ReadAllLines("./prefile.dk");
            int len = lines.Length;
            int lenU = updateLocal.Length;
            string lineLastUpdate = lines[len - 1];
            string lineUpdateLocal = updateLocal[lenU - 1];
            string decryptedstring = Cryptor.Decrypt(lineUpdateLocal);

            if (decryptedstring.Equals("Error"))
            {
                MessageBox.Show("Arquivos corrompidos, por favor reinstale o jogo.", "Erro ao verificar", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            else if (!lineLastUpdate.Equals(decryptedstring))
            {
                attButton.IsEnabled = true;
                updateBar.Value = 0;
                updatingText.Content = "Nova versão disponível!";
            }
            else
            {
                jogarBtn.IsEnabled = true;
                jogarBtn.Background = Brushes.Green;
                attButton.IsEnabled = false;
                updateBar.Value = 100;
                updatingText.Content = "";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Confirmacao para fechar caso haja update em andamento
            if (isUpdating)
            {
                MessageBoxResult result = MessageBox.Show("Há uma atualização em andamento.\nVocê realmente deseja interromper e fechar o programa?", "Confirme sua decisão", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        e.Cancel = true;
                        break;
                }
            }

            // Fecha o game caso o Launcher seja fechado
            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName.ToLower().Contains("dk"))
                {
                    process.Kill();
                }
            }
        }

        private void checkInjector()
        {
            // Checagem de algum programa malicioso
            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName.ToLower().Contains("whook")     ||  // wHooks
                    process.ProcessName.ToLower().Contains("wsyck")     ||  // wSycks
                    process.ProcessName.ToLower().Contains("wpe")       ||  // WPE Pro
                    process.ProcessName.ToLower().Contains("inject")    ||  // Generic Injectors
                    process.ProcessName.ToLower().Contains("cheat")     ||  // Cheat Engine X.XX
                    process.ProcessName.ToLower().Contains("eden")      ||  // EdenBox (C.E. alternative)
                    process.ProcessName.ToLower().Contains("hack")      ||  // Process Hacker
                    process.ProcessName.ToLower().Contains("minimizer") ||  // 4trayminimizer
                    process.ProcessName.ToLower().Contains("4tray")     ||  // 4trayminimizer
                    process.ProcessName.ToLower().Contains("dll")       ||  // DLL Generic Injetors
                    process.ProcessName.ToLower().Contains("extreme"))      // Extreme Injector
                {
                    process.Kill();
                }
            }

            Thread.Sleep(5000);
            Task.Run(() => checkInjector());
        }


    }
}
