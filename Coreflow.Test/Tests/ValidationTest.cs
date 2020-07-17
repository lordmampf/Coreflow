﻿using Coreflow.Storage;
using Coreflow.Storage.ArgumentInjection;
using Coreflow.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coreflow.Test.Tests
{
    [TestClass]
    public class ValidationTest
    {
        private Coreflow mCoreflow;

        [TestInitialize]
        public void Initialize()
        {
            mCoreflow = new Coreflow(
               new SimpleFlowDefinitionFileStorage(@"C:\GitHub\Coreflow\Coreflow.Repository\Flows"),
               new SimpleFlowInstanceFileStorage("FlowInstances"),
               new JsonFileArgumentInjectionStore("Arguments.json")
              );
        }


        [TestMethod]
        public void ValidateFlows()
        {
            foreach (var idefinition in mCoreflow.FlowDefinitionStorage.GetDefinitions())
            {
                var definition = (FlowDefinition)idefinition;

                var result = FlowValidationHelper.Validate(definition);

                var corrector = CorrectorHelper.GetCorrectors(definition, result.Messages);

            }
        }

    }
}
