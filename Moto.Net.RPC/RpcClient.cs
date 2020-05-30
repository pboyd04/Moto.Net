using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Moto.Net.RPC
{
    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel cmdchannel;
        private readonly IModel evtchannel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public event EventHandler GotCall;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            cmdchannel = connection.CreateModel();
            evtchannel = connection.CreateModel();
            replyQueueName = cmdchannel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(cmdchannel);

            evtchannel.ExchangeDeclare(exchange: "radio_events", type: ExchangeType.Fanout);
            string queueName = evtchannel.QueueDeclare().QueueName;
            evtchannel.QueueBind(queue: queueName, exchange: "radio_events", routingKey: "");

            props = cmdchannel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body.ToArray());
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
            EventingBasicConsumer ebc = new EventingBasicConsumer(evtchannel);
            ebc.Received += Ebc_Received;
            evtchannel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: ebc);
        }

        private void Ebc_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            RPCRadioCall call = JsonSerializer.Deserialize<RPCRadioCall>(message);
            this.GotCall?.Invoke(call, null);
        }

        public string Call(RPCMethod method)
        {
            var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(method));
            cmdchannel.BasicPublish(
                exchange: "",
                routingKey: "radio_rpc",
                basicProperties: props,
                body: messageBytes);

            cmdchannel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return respQueue.Take();
        }

        public RPCSystem GetSystem()
        {
            RPCMethod meth = new RPCMethod("GetSystem");
            string res = this.Call(meth);
            return JsonSerializer.Deserialize<RPCSystem>(res);
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
