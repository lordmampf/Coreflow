﻿using Coreflow.Objects.CodeCreatorFactory;
using Coreflow.Web.Controllers;
using Coreflow.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coreflow.Web.Helper
{
    public static class FlowDefinitionModelStorage
    {
        private static Dictionary<Guid, FlowDefinitionModel> mModels = new Dictionary<Guid, FlowDefinitionModel>();
        private static Dictionary<Guid, bool> mUnsavedChanges = new Dictionary<Guid, bool>();

        public static void StoreModel(FlowDefinitionModel pModel, bool pPersistent)
        {
            mModels.Remove(pModel.Identifier);
            mModels.Add(pModel.Identifier, pModel);

            if (pPersistent)
            {
                PersistModel(pModel.Identifier);
            }
            else
            {
                mUnsavedChanges.Remove(pModel.Identifier);
                mUnsavedChanges.Add(pModel.Identifier, true);
            }
        }

        public static void PersistModel(Guid pIdentifier)
        {
            if (!mModels.ContainsKey(pIdentifier)) //not loaded yet
                return;

            FlowDefinition fdef = FlowDefinitionModelMappingHelper.GenerateFlowDefinition(mModels[pIdentifier]);
            fdef.Coreflow = Program.CoreflowInstance;

            Program.CoreflowInstance.FlowDefinitionStorage.Remove(fdef.Identifier);
            Program.CoreflowInstance.FlowDefinitionStorage.Add(fdef);

            string factoryIdentifier = CallFlowCreatorFactory.GetIdentifier(fdef.Identifier);
            var factory = Program.CoreflowInstance.CodeCreatorStorage.GetAllFactories().Single(f => f.Identifier == factoryIdentifier) as CallFlowCreatorFactory;
            factory.FlowName = fdef.Name;

            mUnsavedChanges.Remove(pIdentifier);
            mUnsavedChanges.Add(pIdentifier, false);
        }

        public static bool UnSavedChanges(Guid pIdentifier)
        {
            if (!mUnsavedChanges.ContainsKey(pIdentifier))
                return false;

            return mUnsavedChanges[pIdentifier];
        }

        public static FlowDefinitionModel GetModel(Guid pIdentifier)
        {
            if (mModels.ContainsKey(pIdentifier))
                return mModels[pIdentifier];

            var fdef = Program.CoreflowInstance.FlowDefinitionStorage.GetDefinitions().FirstOrDefault(d => d.Identifier == pIdentifier);
            var fmodel = FlowDefinitionModelMappingHelper.GenerateModel(fdef);

            mModels.Add(fmodel.Identifier, fmodel);
            return fmodel;
        }

        public static void ResetFlow(Guid pIdentifier)
        {
            mModels.Remove(pIdentifier);
            mUnsavedChanges.Remove(pIdentifier);
        }


        public static IEnumerable<FlowDefinition> CombineStoredAndTmpFlows()
        {
            return Program.CoreflowInstance.FlowDefinitionStorage.GetDefinitions().Select(persisted =>
             {
                 if (mUnsavedChanges.ContainsKey(persisted.Identifier))
                     return FlowDefinitionModelMappingHelper.GenerateFlowDefinition(mModels[persisted.Identifier]);
                 return persisted;
             });
        }
    }
}
