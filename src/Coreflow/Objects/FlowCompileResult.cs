﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Coreflow.Objects
{
    public class FlowCompileResult
    {
        public FlowCode FlowCode { get; internal set; }

        public bool Successful { get; internal set; }

        public Assembly ResultAssembly { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public IDictionary<Guid, string> ErrorCodeCreators { get; internal set; }

        internal FlowCompileResult() { }
 
    }
}
