using System;
using System.Messaging;
using System.Windows.Forms;

namespace WindowsFormsApplication5
{
    /// <summary>
    /// Send Message Queue with error handling
    /// </summary>
    public class MessageQueueSender : IMessageQueueManager
    {
        private readonly string _queuePath;
        private System.Messaging.Message _messageHead;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueSender"/> class.
        /// </summary>
        /// <param name="queuePath">The queue path.</param>
        public MessageQueueSender(string queuePath)
        {
            _queuePath = queuePath;
        }

        /// <summary>
        /// Sends the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Send(object obj)
        {
            if (!string.IsNullOrEmpty(_queuePath))
            {
                using (MessageQueue queue = new MessageQueue(_queuePath))
                {
                    if (_messageHead == null)
                    {
                        _messageHead = new System.Messaging.Message();
                    }

                    try
                    {
                        _messageHead.Label = obj.GetType().ToString();
                        _messageHead.Priority = MessagePriority.Normal;
                        _messageHead.Recoverable = false;
                        _messageHead.Body = obj;
                        queue.Send(_messageHead);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error " + ex.ToString());
                    }
                    finally
                    {
                        _messageHead.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the mesasge head.
        /// </summary>
        /// <value>
        /// The mesasge head.
        /// </value>
        public System.Messaging.Message MesasgeHead { get { return _messageHead; } set { _messageHead = value; } }

    }

    public interface IMessageQueueManager
    {
        System.Messaging.Message MesasgeHead { get; set; }
        void Send(object obj);

    }
}
