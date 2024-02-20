using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.Linq;

namespace CLEAN_OP
{
    public partial class Form1 : Form
    {
        private Timer timer;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(sender, e);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Obtener la lista de aplicaciones instaladas
            var applications = Process.GetProcesses().ToList();

            // Crear un diccionario para almacenar las aplicaciones
            var applicationDictionary = new Dictionary<string, Process>();

            // Iterar sobre la lista de aplicaciones
            foreach (var application in applications)
            {
                // Si la aplicación no existe en el diccionario, agregarla
                if (!applicationDictionary.ContainsKey(application.ProcessName))
                {
                    applicationDictionary.Add(application.ProcessName, application);
                }
                // Si la aplicación existe en el diccionario, sumar su tamaño
                else
                {
                    //  applicationDictionary[application.ProcessName].WorkingSet64 += application.WorkingSet64;
                }
            }

            // Ordenar la lista de aplicaciones por nombre
            var sortedApplications = applicationDictionary.Values.ToList();
            sortedApplications.Sort((a, b) => a.ProcessName.CompareTo(b.ProcessName));

            // Mostrar la lista de aplicaciones
            APPS.Items.Clear();
            foreach (var application in sortedApplications)
            {
                APPS.Items.Add(application.ProcessName + " (" + application.WorkingSet64 / 1024 / 1024 + " MB)");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Obtener la aplicación seleccionada
            var application = APPS.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(application))
            {
                MessageBox.Show("Por favor, selecciona una aplicación de la lista.");
                return;
            }

            try
            {
                // Desinstalar la aplicación
                Process.Start("msiexec", $"/x {application} /qn");

                // Mostrar un mensaje de confirmación
                MessageBox.Show($"La aplicación {application} se ha desinstalado correctamente.");

                // Actualizar la lista de aplicaciones
                button1_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo desinstalar la aplicación {application}.\n\nError: {ex.Message}");
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

            // Obtener la información del disco
            var disk = new DriveInfo("C");
            var diskUsage = (double)disk.TotalSize - disk.AvailableFreeSpace;
            var diskPercentage = Math.Round((double)diskUsage / disk.TotalSize * 100.0, 2);
            label3.Text = $"Uso de disco: {diskPercentage}% ({diskUsage / 1024 / 1024} MB de {disk.TotalSize / 1024 / 1024} MB)";

            // Obtener la información de la RAM
            var memory = new PerformanceCounter("Memory", "Available MBytes");
            var memoryUsage = (double)memory.NextValue();
            var memoryPercentage = Math.Round((100.0 - memoryUsage) * 100.0 / memoryUsage, 2);
            label4.Text = $"Uso de RAM: {memoryPercentage}% ({memoryUsage} MB de {memory.NextValue()} MB)";


            // Obtener la información de la CPU
            var cpu = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().FirstOrDefault();
            var cpuLoad = cpu?["LoadPercentage"];
            label7.Text = $"Uso de la CPU: {cpuLoad} %";

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Obtener la información del disco
            var disk = new DriveInfo("C");
            var diskUsage = (double)disk.TotalSize - disk.AvailableFreeSpace;
            var diskPercentage = Math.Round((double)diskUsage / disk.TotalSize * 100.0, 2);
            label3.Text = $"Uso de disco: {diskPercentage}% ({diskUsage / 1024 / 1024} MB de {disk.TotalSize / 1024 / 1024} MB)";

            // Obtener la información de la RAM
            var memory = new PerformanceCounter("Memory", "Available MBytes");
            var memoryUsage = (double)memory.NextValue();
            var memoryPercentage = Math.Round((100.0 - memoryUsage) * 100.0 / memoryUsage, 2);
            label4.Text = $"Uso de RAM: {memoryPercentage}% ({memoryUsage} MB de {memory.NextValue()} MB)";


            // Obtener la información de la CPU
            var cpu = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().FirstOrDefault();
            var cpuLoad = cpu?["LoadPercentage"];
            label7.Text = $"Uso de la CPU: {cpuLoad} %";
        }
    }

    }





