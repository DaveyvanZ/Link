using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;

namespace Link_application
{
    class ConnectionARD
    {
        // Kenmerken
        public SerialPort serialPort;
        private MessageBuilder messageBuilder;

        // Settings
        private const int connectionSpeed = 9600;
        private const int timeoutWrite = 500;
        private const int timeoutRead = 500;
        private const string messageBeginMarker = "#";
        private const string messageEndMarker = "%";
        private const string focusMarker = "@";
        private const string meditatieMarker = "&";
        private const string winkMarker = "$";
        private const string sendMarker = "+";
        private const string linkMarker = "-";
        
        // Constructor
        public ConnectionARD()
        {
            serialPort = new SerialPort();
            serialPort.BaudRate = connectionSpeed;
            serialPort.ReadTimeout = timeoutRead;
            serialPort.WriteTimeout = timeoutWrite;

            messageBuilder = new MessageBuilder(messageBeginMarker, messageEndMarker);
        }

        // Methodes
        public bool SendCheck()
        {
            bool resultaat = false;
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(messageBeginMarker + "+HelloARD" + messageEndMarker);
                    System.Threading.Thread.Sleep(100);
                    resultaat = true;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Kan Link niet bereiken: " + exception.Message);
                }
            }
            return resultaat;
        }

        public void SendMessage(String message)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write(messageBeginMarker + message + messageEndMarker);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Kan Link niet bereiken: " + exception.Message);
                }
            }
        }

        public void ProcessMessages()
        {
            messageBuilder.Append(serialPort.ReadExisting());
            messageBuilder.ProcessBuffer(focusMarker, meditatieMarker, winkMarker, sendMarker);
        }

        public bool CheckLink()
        {
            messageBuilder.Append(serialPort.ReadExisting());
            bool result = messageBuilder.CheckLink(linkMarker, sendMarker);
            return result;
        }

        public void ClearAllMessageData()
        {
            messageBuilder.Clear();
        }
    }
}
