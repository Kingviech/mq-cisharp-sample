﻿using System;
using IBM.WMQ;

namespace MqTest2
{
    public class Program
    {
        static void Main(string[] args)
        {
            MQQueueManager mqQMgr;
            // Try to create a mqqueue manager instance
            try
            {
                mqQMgr = new MQQueueManager("QMA");
            }
            catch (MQException mqe)
            {
                // stop if failed
                Console.WriteLine("create of MQQueueManager ended with " + mqe);
                return;
            }

            // Open the queue
            MQQueue mqQueue;
            try
            {
                mqQueue = mqQMgr.AccessQueue("QMA1", MQC.MQOO_OUTPUT | MQC.MQOO_INPUT_SHARED | MQC.MQOO_INQUIRE);
            }
            catch (MQException mqe)
            {
                // stop if failed
                Console.WriteLine("MQQueueManager::AccessQueue ended with " + mqe);
                return;
            }

            // Prepare hello world message
            var mqMsg = new MQMessage();
            mqMsg.WriteString("Hello from the example client!");
            mqMsg.Format = MQC.MQFMT_STRING;
            var mqPutMsgOpts = new MQPutMessageOptions();

            // Place the message on the queue, using default put message options
            try
            {
                mqQueue.Put(mqMsg, mqPutMsgOpts);
            }
            catch (MQException mqe)
            {
                // report the error
                Console.WriteLine("MQQueue::Put ended with " + mqe);
            }

            // Get message
            mqMsg = new MQMessage();
            var mqGetMsgOpts = new MQGetMessageOptions { WaitInterval = 15 * 1000 };
            // 15 second limit for waiting
            mqGetMsgOpts.Options |= MQC.MQGMO_WAIT;
            try
            {
                mqQueue.Get(mqMsg, mqGetMsgOpts);
                Console.WriteLine(string.Compare(mqMsg.Format, MQC.MQFMT_STRING, StringComparison.Ordinal) == 0 ? mqMsg.ReadString(mqMsg.MessageLength) : "Non-text message");
            }
            catch (MQException mqe)
            {
                // report reason, if any
                if (mqe.Reason == MQC.MQRC_NO_MSG_AVAILABLE)
                {
                    // special report for normal end
                    Console.WriteLine("Wait timeout happened");
                }
                else
                {
                    // general report for other reasons
                    Console.WriteLine("MQQueue::Get ended with " + mqe);

                    // treat truncated message as a failure for this sample
                    if (mqe.Reason == MQC.MQRC_TRUNCATED_MSG_FAILED)
                    {
                        // TODO Handle connection error here
                    }
                }
            }
            try
            {
                //Close the Queue
                mqQueue.Close();

                //Close the Queue Manager
                mqQMgr.Disconnect();
            }
            catch (MQException mqe)
            {
                Console.WriteLine("An IBM MQ error occurred :Completion code " + mqe.CompletionCode + "Reason code " + mqe.ReasonCode);
            }
        }
    }
}
