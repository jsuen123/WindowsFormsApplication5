using System;
using System.Data;
using System.Messaging;
using System.Windows.Forms;

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        private readonly MessageQueue _messageQueue = new MessageQueue();
        private readonly DataTable _dataTable= new DataTable("Employee");

        public Form1(string messageQueuePath)
        {
            InitializeComponent();

            //How to test the following are being executed
            _messageQueue.ReceiveCompleted += new System.Messaging.ReceiveCompletedEventHandler(_messageQueue_ReceiveCompleted);
            _messageQueue.Path = messageQueuePath;
            _messageQueue.BeginReceive(new TimeSpan(0, 0, 3));

            _dataTable.Columns.Add("FirstName", typeof(string));
            _dataTable.Columns.Add("LastName", typeof(string));
            _dataTable.Columns.Add("Department", typeof(string));
            _dataTable.Columns.Add("JobTitle", typeof(string));

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
                typeof(Employee)
            });

            System.Messaging.Message m = mq.EndReceive(e.AsyncResult);

            switch (m.Body.GetType().ToString())
            {
                case "Employee":
                    Process((Employee)m.Body);
                    break;
            }
        }


        private void Process(Employee employee)
        {
            //How to test this?
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Do you need to test this? As it's external dependency        
            _dataTable.Rows.Add(txtInput.Text);
        }
    }

    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
    }
}
