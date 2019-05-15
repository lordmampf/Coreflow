﻿using Coreflow.Api.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Coreflow.Api
{
    public class CoreflowApiClient
    {
        public int Port { get; set; }

        public IPAddress Hostname { get; set; }

        private TcpClient mTcpClient;
        private StreamWriter mStreamWriter;
        private StreamReader mStreamReader;


        public CoreflowApiClient(IPAddress pHostIp, int pPort)
        {
            Port = pPort;
            Hostname = pHostIp;
        }

        public void Connect()
        {
            mTcpClient = new TcpClient();
            mTcpClient.Connect(Hostname, Port);

            mStreamWriter = new StreamWriter(mTcpClient.GetStream(), Encoding.UTF8);
            mStreamReader = new StreamReader(mTcpClient.GetStream(), Encoding.UTF8);
        }

        public LastCompiledFlowInfo SubmitLastCompiledFlowInfoRequest()
        {
            mStreamWriter.WriteLine(nameof(LastCompiledFlowInfo));
            mStreamWriter.Flush();

            string resp = mStreamReader.ReadLine();

            return JsonConvert.DeserializeObject<LastCompiledFlowInfo>(resp);
        }

        public void Close()
        {
            mStreamWriter.Close();
            mStreamReader.Close();
            mTcpClient.Close();
        }

    }
}
