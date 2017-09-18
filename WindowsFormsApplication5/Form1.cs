using System;
using System.Data;
using System.Drawing;
using System.Messaging;
using System.Windows.Forms;

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        private readonly MessageQueue _messageQueue = new MessageQueue();
        private readonly DataTable _dataTable = new DataTable("MachineControls");
        private readonly MessageQueueSender _messageQueueSender;

        public Form1(string messageInQueuePath, MessageQueueSender messageQueueSender)
        {
            InitializeComponent();
            _messageQueueSender = messageQueueSender;

            //How to test the following are being executed
            _messageQueue.ReceiveCompleted += new System.Messaging.ReceiveCompletedEventHandler(_messageQueue_ReceiveCompleted);
            _messageQueue.Path = messageInQueuePath;
            _messageQueue.BeginReceive(new TimeSpan(0, 0, 3));
            _dataTable.Columns.Add("PlcName", typeof(string));
            _dataTable.Columns.Add("ControlIndex", typeof(short));
            _dataTable.Columns.Add("DataType", typeof(string));
            _dataTable.Columns.Add("Address", typeof(uint));
        }

        public string Output
        {
            //Should you use setter/getter to set and get or methods?
            get { return lblOutput.Text; }
            set { lblOutput.Text = value; }
        }

        public string GetOuput()
        {
            //Should you use methods to interact with the UI control or setter/getter?
            return lblOutput.Text;
        }

        public void SetOuput(string s)
        {
            //Should you use methods to interact with the UI control or setter/getter?
            lblOutput.Text = s;
        }


        private void _messageQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            //How to test this?
            MessageQueue mq = (MessageQueue)sender;
            mq.Formatter = new XmlMessageFormatter(new Type[]
            {
                typeof(LightStack),
                typeof(TempGauge)
            });

            System.Messaging.Message m = mq.EndReceive(e.AsyncResult);

            switch (m.Body.GetType().ToString())
            {
                case "LightStack":
                    Process((LightStack)m.Body);
                    break;
                case "TempGauge":
                    Process((TempGauge)m.Body);
                    break;
            }
        }


        public void Process(LightStack message)
        {
            switch (message.LightColour.ToLower())
            {
                case "red":
                    if (message.State)
                        LightstackRedOn();
                    else
                        LightstackRedOff();
                    break;
                case "blue":
                    if (message.State)                    
                        LightstackBlueOn();                    
                    else                    
                        LightstackBlueOff();                    
                    break;
            }
        }

        private void LightstackBlueOn()
        {
            //Shoud you test this and how?
            //Update UI
            if (lblBlue.InvokeRequired)
            {
                lblBlue.Invoke((MethodInvoker)delegate {
                    lblBlue.BackColor = Color.Blue;
                });
            }                     
            else
                lblBlue.BackColor = Color.Blue;
        }

        private void LightstackBlueOff()
        {
            //Shoud you test this and how?
            //Update UI
            if (lblBlue.InvokeRequired)
            {
                lblBlue.Invoke((MethodInvoker)delegate {
                    lblBlue.BackColor = Color.Transparent;
                });
            }
            else
                lblBlue.BackColor = Color.Transparent;

        }

        private void LightstackRedOn()
        {
            //Shoud you test this and how?
            //Update UI
            if (lblRed.InvokeRequired)
            {
                lblRed.Invoke((MethodInvoker)delegate {
                    lblRed.BackColor = Color.Red;
                });
            }
            else
                lblRed.BackColor = Color.Red;
        }

        private void LightstackRedOff()
        {
            //Shoud you test this and how?
            //Update UI
            if (lblRed.InvokeRequired)
            {
                lblRed.Invoke((MethodInvoker)delegate {
                    lblRed.BackColor = Color.Transparent;
                });
            }
            else
                lblRed.BackColor = Color.Transparent;
        }

        public void Process(TempGauge message)
        {
            //Do a number of things
            UpdateTempGauge(message);
        }

        private void UpdateTempGauge(TempGauge message)
        {
            // How do you test the 4 tasks this method need to action
            //1. Query memory table/database to find some data
            DataRow[] rows = _dataTable.Select($@"ControlIndex = {message.Index}");
            if (rows.Length > 0)
            {
                //2. Do some calculations - e.g. Converting temperture from Celsius to Fahrenheit
                double tempInFahrenheit = ConvertCelsiusToFahrenheit(message.Value);
                //2. Update UI (custom control)
                if (lblTempGaugeValue.InvokeRequired)
                {
                    lblTempGaugeValue.Invoke((MethodInvoker)delegate {
                        lblTempGaugeValue.Text = tempInFahrenheit.ToString("0.00");
                    });
                }
                else
                    lblTempGaugeValue.Text = tempInFahrenheit.ToString("0.00");

                //3. Then send to another MSMQ with data gathered from 1. and 2.
                ConvertedTempGauge convertedTempGauge = new ConvertedTempGauge();
                convertedTempGauge.DateTimeLocal = DateTime.Now;
                convertedTempGauge.DateTimeUtc = DateTime.UtcNow;
                convertedTempGauge.ControlIndex = message.Index;
                convertedTempGauge.ControlType = rows[0]["ControlType"].ToString();
                convertedTempGauge.TempInFahrenheit = tempInFahrenheit;
                _messageQueueSender.Send(convertedTempGauge);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Do you need to test this? As it's external dependency  
            string[] input = txtInput.Text.Split(',');      
            _dataTable.Rows.Add(input[0], input[1], input[2], input[3]);
        }

        private double ConvertCelsiusToFahrenheit(double c)
        {
            return ((9.0 / 5.0) * c) + 32;
        }

        private double ConvertFahrenheitToCelsius(double f)
        {
            return (5.0 / 9.0) * (f - 32);
        }
    }

    public class LightStack
    {
        public bool Flashing;
        public string LightColour;
        public bool State;
    }

    public class TempGauge
    {
        public int Index;
        public int MaxValue;
        public int MinValue;
        public int SetpointRangeEnd;
        public int SetpointRangeStart;
        public double Value;
    }

    public class ConvertedTempGauge
    {
        public DateTime DateTimeLocal;
        public DateTime DateTimeUtc;
        public double TempInFahrenheit;
        public string ControlType;
        public int ControlIndex;
    }

}
