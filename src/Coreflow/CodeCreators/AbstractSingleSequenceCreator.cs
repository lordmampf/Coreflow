﻿using Coreflow.Interfaces;
using Coreflow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coreflow.CodeCreators
{
    public abstract class AbstractSingleSequenceCreator : ICodeCreatorContainerCreator, IUiDesignable
    {
        public List<List<ICodeCreator>> CodeCreators { get; set; }

        public ICodeCreatorContainerCreator ParentContainerCreator { get; set; }

        public Guid Identifier { get; set; } = Guid.NewGuid();

        public string FactoryIdentifier { get; set; }

        public virtual string Name => this.GetType().Name;

        public virtual string Icon => "fa-tasks";

        public virtual string Category => null;

        public int SequenceCount { get; } = 1;

        public AbstractSingleSequenceCreator()
        {
        }

        public AbstractSingleSequenceCreator(ICodeCreatorContainerCreator pParentContainerCreator)
        {
            ParentContainerCreator = pParentContainerCreator;
        }

        public abstract void ToSequenceCode(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeBuilder);

        public virtual void WriteHeaderCodeToParentContainer(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeWriter)
        {
        }

        public void ToCode(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeWriter)
        {
            if (CodeCreators != null && CodeCreators.Count > 1)
                throw new Exception("Inconsistent Data");

            pBuilderContext.UpdateCurrentSymbols();

            pCodeWriter.WriteIdentifierTagTop(this);
            pCodeWriter.WriteContainerTagTop(this);

            WriteHeaderCodeToParentContainer(pBuilderContext, pCodeWriter);

            pCodeWriter.AppendLineTop("{"); /* SingleContainer */
            pCodeWriter.AppendLineBottom("}"); /* SingleContainer */

            AddInitializeCode(pBuilderContext, pCodeWriter);
            ToSequenceCode(pBuilderContext, pCodeWriter);
        }

        protected virtual void AddInitializeCode(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeBuilder)
        {
            if (CodeCreators != null && CodeCreators.Count > 0)
                foreach (IVariableCreator varCreator in CodeCreators.First().Select(a => a as IVariableCreator).Where(a => a != null))
                {
                    IVariableCreator existing = FlowBuilderHelper.GetVariableCreatorInInitialScope(this, c => c.VariableIdentifier == varCreator.VariableIdentifier && pBuilderContext.HasLocalVariableName(c));
                    if (existing != null)
                    {
                        pBuilderContext.SetLocalVariableName(varCreator, pBuilderContext.GetLocalVariableName(existing));
                        continue;
                    }

                    varCreator.Initialize(pBuilderContext, pCodeBuilder);
                }
        }

        protected void AddCodeCreatorsCode(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeWriter)
        {
            pCodeWriter.AppendLineTop();
            pCodeWriter.AppendLineTop("{");

            pCodeWriter.AppendLineBottom("}");

            if (CodeCreators != null && CodeCreators.Count > 0)
                foreach (ICodeCreator c in CodeCreators.First())
                {
                    ProcessCodeCreator(pBuilderContext, pCodeWriter, c, this);
                }
        }

        internal static void ProcessCodeCreator(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeWriter, ICodeCreator c, ICodeCreatorContainerCreator pContainer)
        {
            //   pCodeWriter.AppendLineTop("/* start part */");
            int index = pCodeWriter.GetButtomIndex();

            c.ToCode(pBuilderContext, pCodeWriter);

            string bottomPart = pCodeWriter.SubstringButtom(index);

            pCodeWriter.AppendLineTop(bottomPart);
            pCodeWriter.AppendLineTop();

            pCodeWriter.RemoveBottom(index);
            //     pCodeWriter.AppendLineTop("/* end part */");
        }
    }
}
