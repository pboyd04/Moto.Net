using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Moto.Net;
using System.Text.Json;
using Moto.Net.RPC;
using System.Configuration;

namespace MotoMond
{
    public class RPCServer : IDisposable
    {
        protected ConnectionFactory factory;
        protected IConnection conn;
        protected IModel cmdchannel;
        protected IModel evtchannel;
        protected EventingBasicConsumer consumer;
        protected RadioSystem sys;
        protected bool connected;
        private bool disposedValue;

        public RPCServer()
        {
            this.factory = new ConnectionFactory();
            factory.HostName = "localhost";
            try
            {
                conn = factory.CreateConnection();
                cmdchannel = conn.CreateModel();
                evtchannel = conn.CreateModel();
                cmdchannel.ExchangeDeclare(exchange: "radio_events", type: ExchangeType.Fanout);
                evtchannel.QueueDeclare(queue: "radio_rpc", durable: false, exclusive: false, autoDelete: false, arguments: null);
                cmdchannel.BasicQos(0, 1, false);
                consumer = new EventingBasicConsumer(cmdchannel);
                cmdchannel.BasicConsume(queue: "radio_rpc", autoAck: false, consumer: consumer);
                Console.WriteLine("Awating RPC requests...");
                consumer.Received += Consumer_Received;
                connected = true;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
            {
                Console.WriteLine("Failed to connect to RabbitMQ. No gRPC will be available.");
                connected = false;
            }
        }

        public void SetSystem(RadioSystem sys)
        {
            this.sys = sys;
        }

        protected void Consumer_Received(object model, BasicDeliverEventArgs e)
        {
            string response = null;

            var body = e.Body;
            var props = e.BasicProperties;
            var replyProps = cmdchannel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            try
            {
                var message = Encoding.UTF8.GetString(body.ToArray());
                RPCMethod method = JsonSerializer.Deserialize<RPCMethod>(message);
                switch (method.Method)
                {
                    case "GetSystem":
                        response = JsonSerializer.Serialize(sys);
                        break;
                    default:
                        Console.WriteLine(" [.] Unknown Method: {0}", method.Method);
                        response = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" [.] " + ex.Message);
                response = "";
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                cmdchannel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                  basicProperties: replyProps, body: responseBytes);
                cmdchannel.BasicAck(deliveryTag: e.DeliveryTag,
                  multiple: false);
            }
        }

        public void PublishVoiceCall(RadioCall call)
        {
            string message = JsonSerializer.Serialize(call);
            byte[] body = Encoding.UTF8.GetBytes(message);
            if (connected)
            {
                evtchannel.BasicPublish("radio_events", "", basicProperties: null, body: body);
            }
        }

        ~RPCServer()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (cmdchannel != null)
                    {
                        cmdchannel.Dispose();
                        cmdchannel = null;
                    }
                    if (conn != null)
                    {
                        conn.Dispose();
                        conn = null;
                    }
                    this.connected = false;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
