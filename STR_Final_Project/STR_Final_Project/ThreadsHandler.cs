using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;

namespace STR_Final_Project
{
    /// <summary>
    /// Class responsible to manipulate the threads
    /// </summary>
    class ThreadsHandler {
    public int lenBuffer; 
    public double[] arrayRe, arrayIm, arrayFreq, arrayMag, arraySignal, arrayCurrentTime;
    public bool runAquis, runPlot;
    public double dt, sampleRate;
    public int sampleAmount, counterfft;
        
   
    BufferCircular buffer;
    FFT2 fft;
    Chart chartMain;
    SerialPort serialPort;
    
 
    int MSB;
    double LSB;
    double sample, finalSample;
   
    /// <summary>
    /// Constructor of the class
    /// </summary>
    /// <param name="_fft"></param>
    /// <param name="_buffer"></param>
    /// <param name="_chartMain"></param>
    /// <param name="_serialPort"></param>
    public ThreadsHandler(FFT2 _fft, BufferCircular _buffer, Chart _chartMain, SerialPort _serialPort)
    {
        this.buffer = _buffer;
        this.fft = _fft;
        this.chartMain = _chartMain;
        
        this.serialPort = _serialPort;
        lenBuffer = 1024;
        arrayRe = new double [lenBuffer];
        arrayIm = new double[lenBuffer];
        arrayFreq = new double[lenBuffer];
        arrayMag = new double[lenBuffer];
        arraySignal = new double[lenBuffer];
        arrayCurrentTime = new double[lenBuffer];
        runAquis = true;
        runPlot = true;
        sampleRate = 1000;
        sample = 0;
        finalSample = 0;
    
    }

    /// <summary>
    /// Method to collect the samples 
    /// </summary>
    public void funcThreadAquis()
    {
        while (runAquis)
        {

            int sampleAmount = serialPort.BytesToRead; // checking the sample amount at serial port
            if (sampleAmount % 2 != 0) // Making sure to always collect in pairs
            {
                sampleAmount = sampleAmount - 1;
            }
            for (int i = 0; i <= sampleAmount; i++)
            {
               
                LSB = Convert.ToInt32(serialPort.ReadByte()); // Converting bytes to int
                MSB = Convert.ToInt32(serialPort.ReadByte());
                sample = (MSB << 8) + LSB; // Or (MSB*256)+LSB
                    finalSample = ((sample / 1024) )* 5 - 3.1;
                buffer.Write(finalSample); //Collecting the samples and stored at the circular buffer
                }
        }
    }
    /// <summary>
    /// Method to plot the samples of the signal and calculate his FFT
    /// </summary>
    public void funcThreadPlot()
    {
        while (runPlot)
        {
            sampleAmount = buffer.ContSize();
            arraySignal = new double[lenBuffer];
            arrayCurrentTime = new double[lenBuffer];

            if (sampleAmount >= lenBuffer) // Checking if there are enought samples to calculate the FFT 
            {

                for (int i = 0; i < lenBuffer; i++)
                {

                    arraySignal[i] = buffer.Read();// Reading the data
                    arrayRe[counterfft] = arraySignal[i]; // storing the data in arrays to calculate the FFT 
                    arrayIm[counterfft] = 0;
                    counterfft = counterfft + 1;
                }

                    //Calculating the FFT
                    uint logN = (uint)Math.Log((double)lenBuffer, 2.0);
                    fft.init(logN);
                    fft.run(arrayRe, arrayIm, false);

                if (counterfft >= lenBuffer)
                {
                    counterfft = 0;
                }
                //Cleaning the Charts
              //  chartMain.Invoke(new Action(() => chartMain.Series["SeriesFFT"].Points.Clear()));
               // chartMain.Invoke(new Action(() => chartMain.Series["SeriesSinalGerado"].Points.Clear()));

                //Calculating the time
                for (int j = 1; j < lenBuffer; j++)
                {
                    dt = 1.0 / 1000.0;
                    arrayCurrentTime[j] = arrayCurrentTime[j - 1] + dt;
                }

                //Calcuating the Magnitude and the Rate
                for (int k = 0; k < lenBuffer / 2; k++)
                {
                    arrayMag[k] = Math.Sqrt(Math.Pow(arrayRe[k], 2) + Math.Pow(arrayIm[k], 2));
                    arrayFreq[k] = ((k * 1000.0) / lenBuffer);

                }

                //Ploting 
                chartMain.Invoke(new Action(() => chartMain.Series["SeriesSinalGerado"].Points.DataBindXY(arrayCurrentTime, arraySignal)));
                chartMain.Invoke(new Action(() => chartMain.Series["SeriesFFT"].Points.DataBindXY(arrayFreq, arrayMag)));
            }
        }
    }
}
}