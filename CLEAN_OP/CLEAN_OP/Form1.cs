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

        private void button4_Click(object sender, EventArgs e)
        {
            // Get RAM usage information
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            long availableRam = (long)ramCounter.NextValue();
            long totalRam = (long)ramCounter.NextValue();
            long usedRam = totalRam - availableRam;

            // Get disk usage information
            PerformanceCounter diskCounter = new PerformanceCounter("LogicalDisk", "Free Megabytes", "%SystemDrive");
            long availableDisk = (long)diskCounter.NextValue();
            long totalDisk = (long)diskCounter.NextValue();
            long usedDisk = totalDisk - availableDisk;

            // Create chart data
            var chartData1 = new Chart();
            chartData1.Dock = DockStyle.Fill;
            chartData1.BackColor = Color.White;

            var series1 = new Series("RAM Usage");
            series1.ChartType = SeriesChartType.Bar;

            var chartData2 = new Chart();
            chartData2.Dock = DockStyle.Fill;
            chartData2.BackColor = Color.White;

            var series2 = new Series("Disk Usage");
            series2.ChartType = SeriesChartType.Bar;

            // Check if there is any data to display
            if (series1.Points.Count > 0 && series2.Points.Count > 0)
            {
                // Set axis label rotation
                chartData1.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
                chartData2.ChartAreas[0].AxisX.LabelStyle.Angle = -45;

                // Set chart size
                chartData1.Width = panel1.Width;
                chartData1.Height = panel1.Height;
                chartData2.Width = panel2.Width;
                chartData2.Height = panel2.Height;

                // Add data points
                series1.Points.AddXY("Used", usedRam);
                series1.Points.AddXY("Available", availableRam);
                series2.Points.AddXY("Used", usedDisk);
                series2.Points.AddXY("Available", availableDisk);

                // Add chart to panel1
                panel1.Controls.Clear();
                panel1.Controls.Add(chartData1);

                // Add chart to panel2
                panel2.Controls.Clear();
                panel2.Controls.Add(chartData2);
            }
            else
            {
                // Display error message
                MessageBox.Show("No hay datos disponibles.");
                return;
            }
        }
    }
}





