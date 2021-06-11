﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horth.Service.Email.Shared.MsgQueue.Rabbit
{
    public class RabbitQueueName
    {
        private readonly string _queue;
        public RabbitQueueName(string queue)
        {
            _queue = queue;
        }
        public string WorkerExchange => $"{_queue}.exchange";
        public  string RetryExchange => $"{_queue}.deadletter.exchange";
        public  string WorkerQueue => $"{_queue}.queue";
        public  string RetryQueue => $"{_queue}.deadletter.queue";

    }
}
