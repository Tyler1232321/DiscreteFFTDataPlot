// Calling the necessatry libraries and namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Numerics;
using System.Diagnostics;
using System.Threading;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace FFT
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            // Initialize components
            InitializeComponent();
            GraphTimeButton.Enabled = false;
            FFTplot.Enabled = false;
            // Setup the chart options
            #region Chart Setups
            chart1.ChartAreas[0].AxisX.Title = "Time";
            chart1.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;
            chart1.ChartAreas[0].AxisX.TextOrientation = TextOrientation.Horizontal;
            chart1.ChartAreas[0].AxisY.Title = "Volts";
            chart1.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;
            chart1.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Rotated270;

            chart2.ChartAreas[0].AxisX.Title = "Frequency";
            chart2.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;
            chart2.ChartAreas[0].AxisX.TextOrientation = TextOrientation.Horizontal;
            chart2.ChartAreas[0].AxisY.Title = "FFT";
            chart2.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;
            chart2.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Rotated270;
            #endregion 
        }
        // Initialize variables
        int seriesNo2 = 0;
        int seriesNo1 = 0;
        List<complex> Data = new List<complex>();
        Series curr_series2;
        Series curr_series1;
        List<Double> Time = new List<Double>();
        String dataSelect;
        String dataSelect2;
        List<Double> Y = new List<Double>();
        string filename;

        // The complex number class. Not currently in use since it was exported to python
        #region painful Classes
        public class complex
        {
            public double real = 0.0;
            public double imag = 0.0;
            public complex() { }
            public complex(double real, double imag)
            {
                this.real = real;
                this.imag = imag;
            }
            public String ToString()
            {
                String str = real.ToString() + " " + imag.ToString() + "i";
                return str;
            }

            public static complex FromPolar(double r, double radians)
            {
                complex temp = new complex(r * Math.Cos(radians), r * Math.Sin(radians));
                return temp;
            }

            public static complex operator +(complex a, complex b)
            {
                complex temp = new complex(a.real + b.real, a.imag + b.imag);
                return temp;
            }

            public static complex operator -(complex a, complex b)
            {
                complex temp = new complex(a.real - b.real, a.imag - b.imag);
                return temp;
            }

            public static complex operator *(complex a, complex b)
            {
                complex temp = new complex((a.real * b.real) - (a.imag * b.imag), (a.real * b.imag + (a.imag * b.real)));
                return temp;
            }

            public double magnitude
            {
                get
                {
                    return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imag, 2));
                }
            }

            public double phase
            {
                get
                {
                    return Math.Atan(this.imag / this.real);
                }
            }
        }
        /*
        public class data
        {
            public Double time = 0;
            public complex volts = new complex(0, 0);
        }*/
#endregion

        // Function that reads the time data froma files and stores in in the variables
        private void PopulateData(String path, ref List<complex> datalist)
        {
            string line;

            StreamReader file = new StreamReader(path);
            bool time = true;
            while ((line = file.ReadLine()) != null)
            {
                complex data = new complex();
                if (time)
                {
                    try
                    {
                        Time.Add(Double.Parse(line));
                    }
                    catch
                    {
                        time = false;
                        continue;
                    }
                }
                if (!time)
                {
                    data.real = Double.Parse(line);
                    Data.Add(data);
                }
            }
            /*
            while ((line = file.ReadLine()) != null)
            {
                complex data = new complex();
                string[] split = line.Split(Convert.ToChar(9));
                Time.Add(Double.Parse(split[0]));
                data.real = Double.Parse(split[1]);
                datalist.Add(data);
            }
            */
        }
        
        // Chooses the time data to store in a variable
        private void SelectDataButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == DialogResult.OK)
            {
                foreach (String path in openFileDialog1.FileNames)
                {
                    PopulateData(path, ref Data);
                }
                MessageBox.Show("Data Recieved");
            }
            else
            {
                MessageBox.Show("Data not Recieved");
            }
            FFTButton.Enabled = true;
            GraphTimeButton.Enabled = true;
        }

        // Graphs the time data
        private void GraphSelectedData(Chart chart, ref List<complex> Data, ref Series series)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                series.Points.AddXY(Time[i], Data[i].real);
                if (i % 4000 == 0)
                {
                    chart1.Refresh();
                }
            }
        }

        // Calls the function that graphs the data when the graph time button is clicked
        private void GraphTimeButton_Click(object sender, EventArgs e)
        {
            seriesNo2 += 1;
            if (seriesNo2 == 1)
            {
                curr_series2 = chart1.Series["Series" + seriesNo2.ToString()];
                curr_series2.ChartType = SeriesChartType.Spline;
            }
            else
            {
                // create new array
                curr_series2 = new Series("Series" + seriesNo2.ToString(), 15);
                curr_series2.ChartType = SeriesChartType.Spline;
                curr_series2.MarkerColor = Color.Red;

                chart1.Series.Add(curr_series2);
            }

            GraphSelectedData(chart1, ref Data, ref curr_series2);

            MessageBox.Show("Data Graphed");
        }

        #region Attempted FFT
        /*
        public static complex[] FFT(complex[] x)
        {
            int N = x.Length;
            complex[] X = new complex[N];

            complex[] d, D, e, E;

            if (N == 1)
            {
                X[0] = x[0];
                return X;
            }

            int k;

            e = new complex[N / 2];
            d = new complex[N / 2];

            for (k = 0; k < N/2; k++)
            {
                e[k] = x[2 * k];
                d[k] = x[2 * k + 1];
            }

            D = FFT(d);
            E = FFT(e);

            for (k = 0; k < N/2; k++)
            {
                X[k] = E[k] = D[k];
                X[k + N / 2] = E[k] - D[k];
            }

            return X;
        }*/


        public List<complex> FFT(ref List<complex> samples)
        {
            int N = samples.Count();
            if (N == 1)
            {
                return samples;
            }

            int M = N / 2;

            List<complex> even = new List<complex>();
            List<complex> odd = new List<complex>();

            for (int i = 0; i<M; i++)
            {
                even[i] = samples[2 * i];
                odd[i] = samples[2 * i + 1];
            }

            List<complex> Feven = FFT(ref even);
            List<complex> Fodd = FFT(ref odd);

            return null;
        }
        #endregion // This didnt work either
        //  Both of these do nothing, they did not work
        #region Attempt run python script
        private void run_cmd(string cmd, string args)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + args);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = cmd;
            //startInfo.Arguments = args;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = startInfo;
            process.Start();
            //Thread.Sleep(10000);
            StreamReader sr = new StreamReader(dataSelect2);
            while (sr.Peek() == -1)
            {
                continue;
            }
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                {
                    break;
                }
                Y.Add(Convert.ToDouble(line));
                textBox1.Text = line;
                textBox1.Update();
            }
        }
        #endregion // This didnt work at all

        // Function that reads data from a file and stores it in a variable
        private void readData(string file)
        {
            string line;
            
            System.IO.StreamReader sr = new System.IO.StreamReader(file);
            while ((line = sr.ReadLine()) != null)
            {
                Y.Add(Convert.ToDouble(line));
            }
        }

        // Choose the file with the time domain data and the file to write the fft data to, then call the python script to perform the fft, then reads the data to a variable
        private void FFTButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog2.ShowDialog();

            if (dr == DialogResult.OK)
            {
                foreach (String path in openFileDialog2.FileNames)
                {
                    filename = path;
                }
            }
            else
            {
                MessageBox.Show("Data not Recieved");
            }

            dr = openFileDialog3.ShowDialog();
            
            if (dr == DialogResult.OK)
            {
                foreach (String path in openFileDialog3.FileNames)
                {
                    dataSelect2 = path;
                }
                MessageBox.Show("Path Chosen");
            }
            else
            {
                MessageBox.Show("path not chosen");
            }
            var ipy = Python.CreateRuntime();
            dynamic test = ipy.UseFile("FFT.py");
            test.DoFFT(filename,dataSelect2);
            MessageBox.Show("FFT complete, Reading Data");
            readData(dataSelect2);
            FFTplot.Enabled = true;
            MessageBox.Show("Data Read");

            /*
            run_cmd("C:\\Python34\\cmd.exe", "python FFTscript.py " + dataSelect + " " + dataSelect2);
            MessageBox.Show("FFT complete");
            */
        }

        // Plots the data from the variable
        private void GraphFFT(Chart chart, ref Series series)
        {
            int counter = 0;
            for (int i = 0; i < Y.Count/2; i++)
            {
                series.Points.AddXY(counter/8, Y[i]);
                if (i % 4000 == 0)
                {
                    chart.Refresh();
                }
                counter += 1;
            }
        }

        // Sets up the plot and calls the graphing function
        private void FFTplot_Click(object sender, EventArgs e)
        {
            seriesNo1 += 1;
            if (seriesNo1 == 1)
            {
                curr_series1 = chart2.Series["Series" + seriesNo1.ToString()];
                curr_series1.ChartType = SeriesChartType.Spline;
            }
            else
            {
                // create new array
                curr_series1 = new Series("Series" + seriesNo1.ToString(), 15);
                curr_series1.ChartType = SeriesChartType.Spline;
                curr_series1.MarkerColor = Color.Red;

                chart2.Series.Add(curr_series1);
            }

            GraphFFT(chart2, ref curr_series1);

            MessageBox.Show("Data Graphed");
        }
    }
}
