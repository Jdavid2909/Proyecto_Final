using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CLEAN_OP
{
    public partial class Form1 : Form
    {
        private Timer timer;
        public Form1()
        {
            InitializeComponent();

            // Initialize timer
            timer = new Timer();
            timer.Interval = 1000; // Update every second
            timer.Tick += Timer_Tick_Tick;
            timer.Start();

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
            listBox1.Items.Clear();
            foreach (var application in sortedApplications)
            {
                listBox1.Items.Add(application.ProcessName + " (" + application.WorkingSet64 / 1024 / 1024 + " MB)");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Obtener la aplicación seleccionada
            var application = listBox1.SelectedItem?.ToString();
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

        private void Timer_Tick_Tick(object sender, EventArgs e)
        {
            // Get RAM usage information
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            long availableRam = (long)ramCounter.NextValue();
            long totalRam = (int)ramCounter.NextValue();
            long usedRam = totalRam - availableRam;

            // Get disk usage information
            PerformanceCounter diskCounter = new PerformanceCounter("LogicalDisk", "Free Megabytes", "C:");
            long availableDisk = (long)diskCounter.NextValue();
            long totalDisk = (int)diskCounter.NextValue();
            long usedDisk = totalDisk - availableDisk;

            // Get CPU usage information
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float cpuUsage = cpuCounter.NextValue();

            // Get GPU usage information
            // This code uses WMI to get GPU usage information.
            // You may need to install the WMI Code Creator to use this code.

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_GPU");
            foreach (ManagementObject obj in searcher.Get())
            {
                float gpuUsage = (float)obj["PercentShaderTime"];
                label6.Text = $"GPU: {gpuUsage}%";
            }

            // Get temperature information
            // This code uses the OpenHardwareMonitor library to get temperature information.
            // You need to install the OpenHardwareMonitor library to use this code.

            using (var hardware = new Hardware())
            {
                hardware.Update();

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        label7.Text = $"Temperatura: {sensor.Value}°C";
                    }
                }
            }

            // Update labels
            label3.Text = $"Disco: {GetPercentage(usedDisk, totalDisk)}% ({usedDisk} MB / {totalDisk} MB)";
            label4.Text = $"RAM: {GetPercentage(usedRam, totalRam)}% ({usedRam} MB / {totalRam} MB)";
            label5.Text = $"CPU: {cpuUsage}%";
        }
        private string GetPercentage(long used, long total)
        {
            return Math.Round((double)used / (double)total * 100.0, 2).ToString();
        }
    }

    }





