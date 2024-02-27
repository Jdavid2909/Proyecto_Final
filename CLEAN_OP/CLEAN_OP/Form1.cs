using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Data.SqlClient;

namespace CLEAN_OP
{
    public partial class Form1 : Form
    {
        private Timer timer;
        public Form1()
        {
            InitializeComponent();

            this.MaximizeBox = false;
            timer = new Timer();
            timer.Interval = 7000;
            timer.Tick += timer1_Tick;
            //this.WindowState = FormWindowState.Minimized;
            //this.ShowInTaskbar = false;
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(sender, e);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Obtener la lista de procesos
            var processes = Process.GetProcesses();

            // Filtrar solo los procesos que tienen una ventana principal (no son del sistema)
            var userProcesses = processes.Where(p => p.MainWindowHandle != IntPtr.Zero);

            // Crear un diccionario para almacenar las aplicaciones únicas basadas en el nombre del proceso
            var applicationDictionary = new Dictionary<string, Process>();

            // Iterar sobre la lista de procesos del usuario
            foreach (var process in userProcesses)
            {
                // Si la aplicación no existe en el diccionario, agregarla
                if (!applicationDictionary.ContainsKey(process.ProcessName))
                {
                    applicationDictionary.Add(process.ProcessName, process);
                }
                // Si la aplicación existe en el diccionario, sumar su tamaño
                else
                {
                    // No estamos sumando el tamaño en esta versión
                }
            }

            // Ordenar la lista de aplicaciones por nombre
            var sortedApplications = applicationDictionary.Values.ToList();
            sortedApplications.Sort((a, b) => a.ProcessName.CompareTo(b.ProcessName));

            // Mostrar la lista de aplicaciones en el ListBox
            APPS.Items.Clear();
            foreach (var application in sortedApplications)
            {
                APPS.Items.Add(application.ProcessName + " (" + application.WorkingSet64 / 1024 / 1024 + " MB)");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Obtener la aplicación seleccionada en el ListBox
            var selectedApp = APPS.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedApp))
            {
                MessageBox.Show("Por favor, selecciona una aplicación de la lista.");
                return;
            }

            try
            {
                // Obtener el nombre del proceso de la aplicación seleccionada
                var processName = selectedApp.Split('(')[0].Trim();

                // Buscar el proceso correspondiente en la lista de procesos
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                if (process != null)
                {
                    // Cerrar el proceso si está en ejecución
                    process.Kill();
                    process.WaitForExit(); // Esperar a que el proceso se cierre completamente
                }

                // Ejecutar el comando msiexec para desinstalar la aplicación
                var uninstallProcess = Process.Start("msiexec", $"/x {{nombre_del_paquete_MSI}} /quiet");
                uninstallProcess.WaitForExit(); // Esperar a que termine el proceso de desinstalación

                // Mostrar un mensaje de confirmación
                MessageBox.Show($"La aplicación {selectedApp} se ha desinstalado correctamente.");

                // Actualizar la lista de aplicaciones
                button1_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo desinstalar la aplicación {selectedApp}.\n\nError: {ex.Message}");
            }
        }



        private string GetPercentage(long used, long total)
        {
            return Math.Round((double)used / (double)total * 100.0, 2).ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("El proceso se está ejecutando. Por favor, espere...", "Proceso en curso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Verificar si ambos CheckBox están marcados
            if (checkBox1.Checked && checkBox2.Checked)
            {
                // Ejecutar la lógica correspondiente a ambos CheckBox
                CleanTempFiles();
                ShowEdgeDNSInfo();
            }
            else
            {
                MessageBox.Show("Por favor, marca ambos CheckBox para ejecutar esta acción.");
            }

            // Crear la cadena de conexión a la base de datos
            string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

            // Calcular el total de peso limpiado del temp
            long totalTempCleaned = CalculateTotalTempCleaned();

            // Calcular el total de peso limpiado en el navegador
            long totalBrowserCleaned = CalculateTotalBrowserCleaned();

            // Obtener la fecha y hora actual
            DateTime currentDateTime = DateTime.Now;

            // Crear la consulta SQL para insertar los datos
            string query = "INSERT INTO Table_1 (TotalPesoTemp, TotalPesoNavegador, FechaHora) " +
                           "VALUES (@TotalPesoTemp, @TotalPesoNavegador, @FechaHora)";

            // Crear la conexión a la base de datos y el comando SQL
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    // Abrir la conexión
                    connection.Open();

                    // Pasar los parámetros a la consulta SQL
                    command.Parameters.AddWithValue("@TotalPesoTemp", totalTempCleaned);
                    command.Parameters.AddWithValue("@TotalPesoNavegador", totalBrowserCleaned);
                    command.Parameters.AddWithValue("@FechaHora", currentDateTime);

                    // Ejecutar la consulta SQL
                    int rowsAffected = command.ExecuteNonQuery();

                    // Verificar si se insertaron los datos correctamente
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Datos enviados a la base de datos correctamente.");
                    }
                    else
                    {
                        MessageBox.Show("No se pudo insertar los datos en la base de datos.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al enviar datos a la base de datos: " + ex.Message);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

            // Obtener la información del disco
            var disk = new DriveInfo("C");
            var diskUsage = (double)disk.TotalSize - disk.AvailableFreeSpace;
            var diskPercentage = Math.Round((double)diskUsage / disk.TotalSize * 100.0, 2);
            label3.Text = $"Uso de disco: {diskPercentage}% ({diskUsage / 1024 / 1024} MB de {disk.TotalSize / 1024 / 1024} MB)";

            //// Obtener la información de la RAM
            var memory = new PerformanceCounter("Memory", "Available MBytes");
            var memoryUsage = (double)memory.NextValue();
            var memoryPercentage = Math.Round((100.0 - memoryUsage) * 100.0 / memoryUsage, 2);
            label4.Text = $"Uso de RAM: {memoryPercentage}% ({memoryUsage} MB de {memory.NextValue()} MB)";


            //// Obtener la información de la CPU
            var cpu = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().FirstOrDefault();
            var cpuLoad = cpu?["LoadPercentage"];
            label7.Text = $"Uso de la CPU: {cpuLoad} %";

        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                this.BeginInvoke((MethodInvoker)delegate { this.WindowState = FormWindowState.Minimized; this.ShowInTaskbar = false; });
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (timer != null)
            {

                if (timer.Enabled)
                {

                    timer.Stop();
                    button4.Text = "Iniciar";
                }
                else
                {

                    timer.Start();
                    button4.Text = "Detener";
                }
            }
        }

        private void ShowForm(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void ExitForm(object sender, EventArgs e)
        {
            this.Close();
        }


        private void CleanTempFiles()
        {
            var tempPath = Path.GetTempPath();
            try
            {
                // Obtener la lista de archivos temporales
                var files = Directory.GetFiles(tempPath);

                // Eliminar cada archivo de manera segura
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException ex)
                    {
                        // Capturar la excepción si el archivo está siendo utilizado por otro proceso
                        Console.WriteLine($"No se pudo eliminar el archivo '{file}': {ex.Message}");
                    }
                }

                // Eliminar los directorios vacíos después de eliminar los archivos
                var directories = Directory.GetDirectories(tempPath);
                foreach (var directory in directories)
                {
                    try
                    {
                        Directory.Delete(directory);
                    }
                    catch (IOException ex)
                    {
                        // Capturar la excepción si el directorio no está vacío o está siendo utilizado
                        Console.WriteLine($"No se pudo eliminar el directorio '{directory}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción no esperada al obtener la lista de archivos
                Console.WriteLine($"Error al obtener la lista de archivos temporales: {ex.Message}");
            }
        }


        private void ShowEdgeDNSInfo()
        {
            Process.Start("microsoft-edge:", "edge://net-internals/#dns");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Obtener la información del disco
            var disk = new DriveInfo("C");
            var diskTotalSizeMB = disk.TotalSize / (1024 * 1024);
            var diskAvailableSizeMB = disk.AvailableFreeSpace / (1024 * 1024);
            var diskUsageMB = diskTotalSizeMB - diskAvailableSizeMB;
            var diskPercentage = Math.Round((double)diskUsageMB / diskTotalSizeMB * 100.0, 2);
            label3.Text = $"Uso de disco: {diskPercentage}% ({diskUsageMB} MB de {diskTotalSizeMB} MB)";

            // Obtener la información de la RAM
            var memoryAvailableMB = (double)(new PerformanceCounter("Memory", "Available MBytes")).NextValue();
            var memoryTotalMB = (double)(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024 * 1024)); // Tamaño total de RAM en MB
            var memoryUsageMB = memoryTotalMB - memoryAvailableMB; // Uso de RAM en MB
            var memoryPercentage = Math.Round(memoryUsageMB / memoryTotalMB * 100.0, 2);
            label4.Text = $"Uso de RAM: {memoryPercentage}% ({memoryUsageMB} MB de {memoryTotalMB} MB)";

            // Obtener la información de la CPU
            var cpuLoad = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().FirstOrDefault()?["LoadPercentage"];
            label7.Text = $"Uso de la CPU: {cpuLoad} %";
        }

        private long CalculateTotalTempCleaned()
        {

            string tempPath = Path.GetTempPath();


            string[] tempFiles = Directory.GetFiles(tempPath);


            long totalTempSize = 0;
            foreach (string file in tempFiles)
            {
                FileInfo fileInfo = new FileInfo(file);
                totalTempSize += fileInfo.Length;
            }


            return totalTempSize;
        }

        private long CalculateTotalBrowserCleaned()
        {

            string browserCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NombreDelNavegador", "Cache");


            if (Directory.Exists(browserCachePath))
            {

                string[] browserCacheFiles = Directory.GetFiles(browserCachePath);


                long totalBrowserCacheSize = 0;
                foreach (string file in browserCacheFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalBrowserCacheSize += fileInfo.Length;
                }


                return totalBrowserCacheSize;
            }
            else
            {

                return 0;
            }
        }
    }
}





