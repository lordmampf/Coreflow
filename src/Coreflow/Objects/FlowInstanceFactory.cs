﻿using Coreflow.Helper;
using Coreflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Coreflow.Objects
{
    public class FlowInstanceFactory : IDisposable
    {
        public delegate ICompiledFlow FlowCreator();


        public Coreflow Coreflow { get; protected set; }

        public Guid DefinitionGuid { get; protected set; }

        protected FlowCreator mCreator;

        public bool IsDisposed { get; protected set; }

        internal FlowInstanceFactory(Coreflow pCoreFlow, Guid pDefinitionGuid, Type pDynamicFlowType)
        {
            Coreflow = pCoreFlow;
            DefinitionGuid = pDefinitionGuid;

            ConstructorInfo emptyConstructor = pDynamicFlowType.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", pDynamicFlowType, Type.EmptyTypes, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            //   ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);

            mCreator = (FlowCreator)dynamicMethod.CreateDelegate(typeof(FlowCreator));
        }

        public ICompiledFlow NewInstance()
        {
            if (IsDisposed)
                throw new Exception("Factory is disposed, because no longer up to date");

            return mCreator();
        }

        public IDictionary<string, object> RunInstance(IDictionary<string, object> pArguments = null)
        {
            ICompiledFlow flow = NewInstance();

            FlowInstance flowInstance = new FlowInstance()
            {
                DefinitionIdentifier = DefinitionGuid,
                StartTime = DateTime.UtcNow,
                Identifier = flow.InstanceId
            };

            Coreflow.FlowInstanceStorage.Add(flowInstance);

            if (pArguments != null)
                flow.SetArguments(pArguments);

            flow.Run();

            flowInstance.EndTime = DateTime.UtcNow;

            Coreflow.FlowInstanceStorage.Update(flowInstance);

            return flow.GetArguments();
        }

        public void Dispose()
        {
            mCreator = null;
            IsDisposed = true;
        }
    }
}
