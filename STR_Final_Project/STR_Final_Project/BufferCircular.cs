using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STR_Final_Project
{
    /// <summary>
    /// The CirculaBuffer class provides a data structure for creating a dynamic circular buffer (also known as a cyclic buffer or a ring buffer). 
    /// </summary>
    public class BufferCircular
    {

        public double[] buff; 
        public int pwr, prd, cont, size; 
     

        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="_size"></param>
        public BufferCircular(int _size)
        {
           this.size = _size; //Size of buffer
            this.buff = new double[size]; //Buffer is a vector that store the signal data
           pwr = 0; // pointer for writing the data 
           prd = 0; // pointer for reading the data
           cont = 0;//counter
        }
        /// <summary>
        /// This method writes or replaces one or more values on the buffer
        /// </summary>
        /// <param name="_newValue"></param>
        public void Write(double _newValue)
        {
           
            buff[pwr] = _newValue;
            cont++; // Uptade counter position
            pwr++; // Uptade pointer position

            if (pwr >= size)
            {
                pwr = 0; //Starting the vector again when it reaches the last position
            }
        }

        /// <summary>
        /// This method reads one or more values on the buffer 
        /// </summary>
        /// <returns></returns>
        public double Read()
        {
            //if (cont >0) 
            double valueRd = buff[prd];
            cont--; //Update the counter position
            prd++; // Uptade the pointer position
           
            if (prd >= size)// Starting the vector again when it reaches the last position
            {
                prd = 0;
            }
            return valueRd;
            
        }

        /// <summary>
        /// This method return the size of the buffer
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return size;
        }

        /// <summary>
        /// This method return the number of data that are stored on the buffer
        /// </summary>
        /// <returns></returns>
        public int ContSize()
        {
            return cont;
        }
    }
}


