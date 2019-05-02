﻿using Coreflow.Helper;
using Coreflow.Interfaces;
using Coreflow.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coreflow.Storage
{
    public class SimpleFlowInstanceFileStorage : IFlowInstanceStorage
    {
        public string FilePath { get; }

        public SimpleFlowInstanceFileStorage(string pFilePath)
        {
            FilePath = pFilePath;
            Directory.CreateDirectory(pFilePath);
        }

        public void Add(FlowInstance pFlowInstance)
        {
            string serialized = FlowInstanceSerializer.Serialize(pFlowInstance);
            File.WriteAllText(Path.Combine(FilePath, pFlowInstance.Identifier.ToString()), serialized);
        }

        public void Update(FlowInstance pFlowInstance)
        {
            File.Delete(Path.Combine(FilePath, pFlowInstance.Identifier.ToString()));
            Add(pFlowInstance);
        }

        public IEnumerable<FlowInstance> GetInstances()
        {
            List<FlowInstance> ret = new List<FlowInstance>();

            foreach (string file in Directory.GetFiles(FilePath))
            {
                string text = File.ReadAllText(file);
                ret.Add(FlowInstanceSerializer.Deserialize(text));
            }

            return ret;
        }

        public void Dispose()
        {
        }
    }
}
