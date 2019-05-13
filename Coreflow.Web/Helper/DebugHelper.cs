﻿using Coreflow.Helper;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Coreflow.Web.Helper
{
    public static class DebugHelper
    {
        private static DebugProtocolHost mClient = null;
        private static Process mDebuggerProcess = null;

        private static List<int> mBreakPoints = new List<int>();

        public static void Attach(int pProcessId)
        {
            if (mDebuggerProcess != null)
                Detach();


            mBreakPoints.Clear();

            ProcessStartInfo psi2 = new ProcessStartInfo();
            psi2.FileName = @"D:\tmp\netcoredbg-master\bin\netcoredbg.exe";
            psi2.Arguments = "--interpreter=vscode --engineLogging=log.log"; // 
            psi2.UseShellExecute = false;
            //   psi2.Verb = "runas";

            psi2.RedirectStandardInput = true;
            psi2.RedirectStandardOutput = true;

            mDebuggerProcess = Process.Start(psi2);

            mClient = new DebugProtocolHost(mDebuggerProcess.StandardInput.BaseStream, mDebuggerProcess.StandardOutput.BaseStream, true);
            mClient.EventReceived += Client_EventReceived;
            mClient.Run();

            InitializeRequest ir = new InitializeRequest();
            ir.ClientID = "CoreflowDebugger";
            ir.AdapterID = "CoreflowAdapter";
            ir.ColumnsStartAt1 = true;
            ir.PathFormat = InitializeArguments.PathFormatValue.Path;
            ir.SupportsVariablePaging = true;
            ir.SupportsVariableType = true;
            ir.SupportsRunInTerminalRequest = true;

            var repsonse = mClient.SendRequestSync(ir);

            AttachRequest ar = new AttachRequest();
            ar.Args.ConfigurationProperties.Add("processId", pProcessId);
            mClient.SendRequestSync(ar);

            ConfigurationDoneRequest cr = new ConfigurationDoneRequest();
            mClient.SendRequestSync(cr);
        }

        public static IEnumerable<DebugThread> GetThreads()
        {
            ThreadsRequest tr = new ThreadsRequest();
            var tresponse = mClient.SendRequestSync(tr);

            return tresponse.Threads.Select(t => new DebugThread(t.Name, t.Id));
        }

        public static void Pause(int pThreadId)
        {
            PauseRequest pr = new PauseRequest();

            mClient.SendRequestSync(pr);
        }

        public static void GetStackTrace(int pThreadId)
        {
            StackTraceRequest str = new StackTraceRequest();
            str.ThreadId = pThreadId;
            var response = mClient.SendRequestSync(str);
        }

        public static void AddBreakPoint(int pLine)
        {
            mBreakPoints.Add(pLine);
            UpdateBreakPoints();
        }

        public static void RemoveBreakPoint(int pLine)
        {
            mBreakPoints.Remove(pLine);
            UpdateBreakPoints();
        }

        private static void UpdateBreakPoints()
        {
            string file = Path.GetFullPath(FlowCompilerHelper.LAST_COMPILED_CODE_FILE);

            SetBreakpointsRequest br = new SetBreakpointsRequest();
            br.Source = new Source()
            {
                Name = "LastCompiledCode.cs",
                Path = @"C:\GitHub\Coreflow\Coreflow.Host\bin\Debug\netcoreapp3.0\LastCompiledCode.cs",
            };
            br.SourceModified = false;
            br.Lines = mBreakPoints;

            br.Breakpoints = mBreakPoints.Select(b => new SourceBreakpoint(b)).ToList();

            var bresponse = mClient.SendRequestSync(br);
        }


        public static void Detach()
        {
            DisconnectRequest dr = new DisconnectRequest();
            mClient.SendRequestSync(dr);

            mClient.Stop();

            mDebuggerProcess.Kill();

            mClient = null;
            mDebuggerProcess = null;
        }

        private static void Client_EventReceived(object sender, EventReceivedEventArgs e)
        {
            Console.WriteLine(e.EventType);
        }
    }
}
