  /* 
    * Federal University of Uberlandia
    * Faculty of Electrical Engineering
    * Biomedical Engineering Lab
    * Author: Débora Pereira Salgado
    * contact: deborapsalgado@gmail.com
    * 
    *
    * -----------------------------------------------
    * Description: Using timer and handshake communication 
    * to collect an analog signal and send to a program through
    * this serial protocol
    * ----------------------------------------------- 
    */

//Libraries
#include <TimerOne.h>  

//Parameters
double sampPeriod = 1000;// Period in microseconds 
byte LSB, MSB; //Variables to store the data in byte.

void setup() {
 
Serial.begin(115200);// Initializing the serial 
pinMode(13,OUTPUT);
Timer1.initialize(sampPeriod);//specify the timer's period here (in microseconds)
Timer1.attachInterrupt(getData); //Calls a function at the specified interval in microseconds 
}

/*
 * Method to read the analog signal and send the data in bytes
 */
void getData(){

  int sinal = analogRead(A0); // signal with the length in 2 bytes

    LSB = sinal & 0x00FF; // store the  less  significative byte (8 bits)
    MSB = sinal >> 8; // store the more significative byte (8 bits)
   
  Serial.write(LSB); // Sending the data through serial port
  Serial.write(MSB); 

  
}

void loop() {

  //HandShake Serial Communication

     if(Serial.available() > 0)
  {
      switch(Serial.read()){
        case 'I':
        
          digitalWrite(13,HIGH);
          Timer1.initialize(sampPeriod);// Starting the timer when is asked
        break;
        case 'P':
           digitalWrite(13,LOW); 
           Timer1.stop(); // Stop the timer when is asked
        break;
  }
  }
  

}
