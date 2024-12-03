using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;
using System.Threading;

namespace STR_Final_Project
{
    /* 
    * Federal University of Uberlandia
    * Faculty of Electrical Engineering
    * Biomedical Engineering Lab
    * Author: Débora Pereira Salgado
    * contact: deborapsalgado@gmail.com
    * 
    * Reference: github.com/andreinakagawa/biomedical_engineering
    * -----------------------------------------------
    * Description: Using threads to collect and plot an analog signal 
    * the signal is collected by any EMG/EEG arduino/shield and it was
    *  send through a handshake serial communication to this program
    * ----------------------------------------------- 
    */
    public partial class Form : System.Windows.Forms.Form

    {
       
        //Declaring objects 
        BufferCircular buffer;
        FFT2 fft;
        ThreadsHandler myThreadsHandler;
        Thread myThreadAquis, myThreadPlot; 

        //Declaring flags to manipulate the buttons to "connect" and "start aquisiton"
        public bool flagStart, flagConnect;


        // Declaring objects to manipulate the Chart
        public ChartArea chartAreaMain, chartAreaFFT;
        public Series seriesSinalGerado, seriesFFT;

       // Defining constants of type String
        public static class mainConstant
        {
            public const string chartAreaMain_Name = "ChartAreaMain";
            public const string seriesSinalGerado_Name = "SeriesSinalGerado";
            public const string chartAreaMainFFT_Name = "ChartAreaFFT";
            public const string seriesFFT_Name = "SeriesFFT";

        }

        public Form()
        {
            InitializeComponent();
            buffer = new BufferCircular(2000); //Creating the buffer object with size of 2000
            fft = new FFT2(); // Creating the fft object
            btnStart.Enabled = false; 
            btnConnect.Enabled = false; 
        }

        /// <summary>
        /// Method to format The Signal Area Chart control by setting some the individual properties
        /// </summary>
        private void setChartSeries_Main()
        {
           
            chartAreaMain = new ChartArea(); // Criating a new chart
            chartAreaMain.Name = mainConstant.chartAreaMain_Name;
            chartMain.ChartAreas.Add(chartAreaMain); // Adding the chart into the ChartArea 
            chartMain.BackColor = Color.WhiteSmoke; //setting the background color
           // chartAreaMain.BackColor = Color.GhostWhite;


            //Setting the properties for  X axis 
            chartAreaMain.AxisX.LabelStyle.Format = "{0:0.00}";
            chartAreaMain.AxisX.LabelStyle.Font = new Font("Arial", 8.0F, FontStyle.Regular);
            chartAreaMain.AxisX.MajorTickMark.Enabled = false;
            chartAreaMain.AxisX.MajorGrid.LineColor = Color.Black;
            chartAreaMain.AxisX.MajorGrid.Enabled = true;
            chartAreaMain.AxisX.MinorTickMark.Enabled = false;
            chartAreaMain.AxisX.MinorGrid.Enabled = false;
            chartAreaMain.AxisX.MinorGrid.LineColor = Color.LightGray;
            chartAreaMain.AxisX.MinorGrid.Enabled = true;
            chartAreaMain.AxisX.Minimum = 0;
            chartAreaMain.AxisX.Maximum = (Convert.ToDouble(1024) / 1000.0);


            // Setting the properties fo Y axis
            chartAreaMain.AxisY.LabelStyle.Format = "{0:0.00}";
            chartAreaMain.AxisY.LabelStyle.Font = new Font("Arial", 8.0F, FontStyle.Regular);
            chartAreaMain.AxisY.MajorTickMark.Enabled = false;
            chartAreaMain.AxisY.MajorGrid.LineColor = Color.Black;
            chartAreaMain.AxisY.MajorGrid.Enabled = true;
            chartAreaMain.AxisY.MinorTickMark.Enabled = false;
            chartAreaMain.AxisY.MinorGrid.Enabled = false;
            chartAreaMain.AxisY.MinorGrid.LineColor = Color.LightGray;
            chartAreaMain.AxisY.MinorGrid.Enabled = true;
            chartAreaMain.AxisY.Minimum = -5;
            // chartAreaMain.AxisY.Maximum = 1024;
            chartAreaMain.AxisY.Maximum = 5;
            chartAreaMain.Name = "charAreaMain";

            // Setting the title
            Title titleChartConfig = new Title();
            titleChartConfig.Name = "TitleChartConfig";
            titleChartConfig.Text = "Collected signal and your FFT";
            titleChartConfig.DockedToChartArea = chartAreaMain.Name;
            titleChartConfig.IsDockedInsideChartArea = false;
            titleChartConfig.Docking = Docking.Top;
            titleChartConfig.Font = new Font("Arial", 12, FontStyle.Bold);
            titleChartConfig.ForeColor = Color.Black;
            chartMain.Titles.Add(titleChartConfig);

            // Setting the Signal's Serie
            seriesSinalGerado = new Series(mainConstant.seriesSinalGerado_Name);
            seriesSinalGerado.ChartType = SeriesChartType.FastLine; // Chart type - by points
            seriesSinalGerado.Color = Color.Blue; 
            seriesSinalGerado.ChartArea = chartAreaMain.Name; //Link series 'signal generated series' to Chart Area
            chartMain.Series.Add(seriesSinalGerado);
            seriesSinalGerado.Name = "SeriesSinalGerado";
        }

        /// <summary>
        /// Method to format The FFT Area Chart control by setting some the individual properties
        /// </summary>
        private void setChartSeries_FFT()
        {

            chartAreaFFT = new ChartArea(); // Criating a new Chart
            chartAreaFFT.Name = mainConstant.chartAreaMainFFT_Name;
            chartMain.ChartAreas.Add(chartAreaFFT); //  Adding the chart into the ChartArea 
            chartMain.BackColor = Color.GhostWhite;
           // chartAreaFFT.BackColor = Color.WhiteSmoke;

            // Setting the properties for  X axis X axis 
            chartAreaFFT.AxisX.LabelStyle.Format = "{0:0.00}";
            chartAreaFFT.AxisX.LabelStyle.Font = new Font("Arial", 8.0F, FontStyle.Regular);
            chartAreaFFT.AxisX.MajorTickMark.Enabled = false;
            chartAreaFFT.AxisX.MajorGrid.LineColor = Color.Black;
            chartAreaFFT.AxisX.MajorGrid.Enabled = true;
            chartAreaFFT.AxisX.MinorTickMark.Enabled = false;
            chartAreaFFT.AxisX.MinorGrid.Enabled = false;
            chartAreaFFT.AxisX.MinorGrid.LineColor = Color.LightGray;
            chartAreaFFT.AxisX.MinorGrid.Enabled = true;


            //Setting the properties for  Y axis
            chartAreaFFT.AxisY.LabelStyle.Format = "{0:0.00}";
            chartAreaFFT.AxisY.LabelStyle.Font = new Font("Arial", 8.0F, FontStyle.Regular);
            chartAreaFFT.AxisY.MajorTickMark.Enabled = false;
            chartAreaFFT.AxisY.MajorGrid.LineColor = Color.Black;
            chartAreaFFT.AxisY.MajorGrid.Enabled = true;
            chartAreaFFT.AxisY.MinorTickMark.Enabled = false;
            chartAreaFFT.AxisY.MinorGrid.Enabled = false;
            chartAreaFFT.AxisY.MinorGrid.LineColor = Color.LightGray;
            chartAreaFFT.AxisY.MinorGrid.Enabled = true;
            chartAreaFFT.AxisY.Maximum = 70;
            chartAreaFFT.AxisY.Minimum = 0;


            //Setting the FFT's Serie
            seriesFFT = new Series(mainConstant.seriesFFT_Name);
            seriesFFT.ChartType = SeriesChartType.Column; // Chart type - by points
            seriesFFT.Color = Color.Blue; 
            seriesFFT.ChartArea = chartAreaFFT.Name; // Link series 'seriesFFT' to Chart Area
            chartMain.Series.Add(seriesFFT);
        }



        private void Form_Load(object sender, EventArgs e)
        {

            setChartSeries_Main();
            setChartSeries_FFT();

            myThreadsHandler = new ThreadsHandler(fft, buffer, chartMain, serialPort); // creating the myThreadHandler object to manipulate the threads

            myThreadAquis = new Thread(myThreadsHandler.funcThreadAquis);// creating the thread to collect the signal
            myThreadAquis.Priority = ThreadPriority.Normal;//setting the thread's priority
         
            myThreadPlot = new Thread(myThreadsHandler.funcThreadPlot);// creating the thread to plot the signal and calculate  the FFT 
            myThreadPlot.Priority = ThreadPriority.Normal; //setting the thread's priority



            string[] nomesPortas = System.IO.Ports.SerialPort.GetPortNames(); //Getting the names of USB ports availables 
            for (int i = 0; i < nomesPortas.Length; i++)
            {
                comboBoxPortas.Items.Add(nomesPortas[i]); //Adding the names of USB ports into a comboBox
            }
        }


        /// <summary>
        /// Button responsible to open and close a serialPort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            if (!flagConnect)
            {
                flagConnect = true;
                btnConnect.Text = "Desconnect ";
                btnConnect.BackColor = Color.Red;
                serialPort.PortName = comboBoxPortas.Items[comboBoxPortas.SelectedIndex].ToString();
                serialPort.Open();// Opening the serial 
                if (serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();//Cleaning the serial buffer
                    serialPort.DiscardOutBuffer();
                  //Print a message
                    MessageBox.Show("Connection successfully completed!", "Attention",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                flagConnect = false;
                btnConnect.Text = "Connect";
                btnConnect.BackColor = Color.White;
                if (serialPort.IsOpen)
                {
                    //Close the serial
                    serialPort.Close();

                    //Checks if the port was closed and print a message
                    if (serialPort.IsOpen)
                    {
                        MessageBox.Show("Error: Closing the connection refused!");
                    }
                    else
                        MessageBox.Show("Connection closed!");
                }
            }

        }

        /// <summary>
        /// Event to close the windown 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void comboBoxPortas_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnConnect.Enabled = true;
        }

        /// <summary>
        /// Button responsible to start the threads and the connection 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
           
            if (!flagStart)
            {
                flagStart = true;
                btnStart.Text = "Stop";
                btnStart.BackColor = Color.Red;
                myThreadAquis.Start();//Starting the threafs
                myThreadPlot.Start();
                serialPort.WriteLine("I");//Sending a message to beggin the aquisition
            }
            else
            {
                flagStart = false;
                btnStart.Text = "Start";
                btnStart.BackColor = Color.White;
                myThreadsHandler.runAquis = false; //We can't stop the threads, this situation we just disabled their functions.
                myThreadsHandler.runPlot = false;
                serialPort.WriteLine("P"); // Sending a message to stop 
                
            }          
         }
    }
}