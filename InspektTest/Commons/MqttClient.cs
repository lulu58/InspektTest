using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;

namespace WFDashboard.Tools
{
    // public sealed class Managed_Client_Simple_Samples
    public sealed class MqttClient
    {
        // Licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        // See the LICENSE file in the project root for more information.

        const string MQTT_BROKER_HOSTNAME = "broker.hivemq.com";
        const string MQTT_CLIENT_ID = "mqttdash-4ce12cba";
        const string MQTT_TOPIC1 = "/Lulu/home/temperature";
        //const string MQTT_TOPIC2 = "/Lulu/work/temperature";
        const string MQTT_TOPIC2 = "/Lulu/ch/lamp";

        IManagedMqttClient managedMqttClient;

        public async Task Connect_Client()
        {
            Debug.WriteLine("MqttClient.Connect_Client()");
            /*
             * This sample creates a simple managed MQTT client and connects to a public broker.
             *
             * The managed client extends the existing _MqttClient_. It adds the following features.
             * - Reconnecting when connection is lost.
             * - Storing pending messages in an internal queue so that an enqueue is possible while the client remains not connected.
             */

            var mqttFactory = new MqttFactory();
            managedMqttClient = mqttFactory.CreateManagedMqttClient();

            managedMqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedAsync;

            //using (var managedMqttClient = mqttFactory.CreateManagedMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(MQTT_BROKER_HOSTNAME)    //"broker.hivemq.com")
                    //.WithClientId(MQTT_CLIENT_ID)
                    .Build();

                var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                    .WithClientOptions(mqttClientOptions)
                    .Build();

                await managedMqttClient.StartAsync(managedMqttClientOptions);

                // The application message is not sent. It is stored in an internal queue and
                // will be sent when the client is connected.
                //await managedMqttClient.EnqueueAsync("Topic", "Payload");

                Debug.WriteLine(" - The managed MQTT client is connected.");

                await SubscribeAsync();

                // Wait until the queue is fully processed.
                SpinWait.SpinUntil(() => managedMqttClient.PendingApplicationMessagesCount == 0, 10000);

                Debug.WriteLine($" - Pending messages = {managedMqttClient.PendingApplicationMessagesCount}");
            }
        }

        /// <summary>
        /// Subscribe to 2 topics
        /// </summary>
        /// <returns></returns>
        private async Task SubscribeAsync()
        {
            if (managedMqttClient != null)
            {
                var mqttFactory = new MqttFactory();

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                   .WithTopicFilter(f => { f.WithTopic(MQTT_TOPIC1); })
                   .WithTopicFilter(f => { f.WithTopic(MQTT_TOPIC2); })
                .Build();

                await managedMqttClient.SubscribeAsync(topicFilters);

                await managedMqttClient.SubscribeAsync(MQTT_TOPIC1, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    //.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                Debug.WriteLine("MQTT client subscribed to topic. " + MQTT_TOPIC1);

                await managedMqttClient.SubscribeAsync(MQTT_TOPIC2, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                //.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                Debug.WriteLine("MQTT client subscribed to topic. " + MQTT_TOPIC2);

            }
        }

        /// <summary>
        /// Event: Broker has sent a message
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            MqttApplicationMessage msg = arg.ApplicationMessage;
            if (msg != null)
            {
                Console.WriteLine("Received application message.");
                string topic = msg.Topic;
                string payload = msg.ConvertPayloadToString();
                // show in list
                //ShowTopic(topic, payload);

                if (topic == MQTT_TOPIC1)
                {
                    Debug.WriteLine(" - MQTT_TOPIC1: " + payload);

                }

                if (topic == MQTT_TOPIC2)
                {
                    Debug.WriteLine(" - MQTT_TOPIC2: " + payload);
                    //if (payload == "0")
                    //    listBox1.BackColor = Color.Red;
                    //else
                    //    listBox1.BackColor = Color.Green;
                }
            }
            return Task.CompletedTask;
        }

    }
}

https://stackoverflow.com/questions/7880850/how-do-i-make-an-event-in-the-usercontrol-and-have-it-handled-in-the-main-form

You need to create an event handler for the user control that is raised when an event from within the user control is fired. This will allow you to bubble the event up the chain so you can handle the event from the form.

When clicking Button1 on the UserControl, i'll fire Button1_Click which triggers UserControl_ButtonClick on the form:

User control:

[Browsable(true)]
[Category("Action")]
[Description("Invoked when user clicks button")]
public event EventHandler ButtonClick;

protected void Button1_Click(object sender, EventArgs e)
{
    //bubble the event up to the parent
    if (this.ButtonClick != null)
        this.ButtonClick(this, e);
}
Form:

UserControl1.ButtonClick += new EventHandler(UserControl_ButtonClick);

protected void UserControl_ButtonClick(object sender, EventArgs e)
{
    //handle the event 
}



Newer Visual Studio versions suggest that instead of 
    if (this.ButtonClick!= null) this.ButtonClick(this, e); 
you can use 
    ButtonClick?.Invoke(this, e);
, which does essentially the same, but is shorter.

    The Browsable attribute makes the event visible in Visual Studio's designer (events view), 
    Category shows it in the "Action" category, and Description provides a description for it. 
    You can omit these attributes completely, but making it available to the designer it is 
    much more comfortable, since VS handles it for you.
