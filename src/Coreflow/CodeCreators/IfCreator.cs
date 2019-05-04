﻿using Coreflow.Interfaces;
using Coreflow.Objects;
using System.Collections.Generic;

namespace Coreflow.CodeCreators
{
    public class IfCreator : AbstractSingleSequenceCreator, IParametrized
    {
        public List<IArgument> Arguments { get; set; } = new List<IArgument>();

        public IfCreator()
        {
        }

        public IfCreator(ICodeCreatorContainerCreator pParentContainerCreator) : base(pParentContainerCreator)
        {
        }

        public override string Name => "If";

        public override string Icon => "fa-check";

        public override string Category => "Basic";

        public override void ToSequenceCode(FlowBuilderContext pBuilderContext, FlowCodeWriter pCodeBuilder, ICodeCreatorContainerCreator pContainer)
        {
            pCodeBuilder.AppendLineTop("if (");
            Arguments[0].ToCode(pBuilderContext, pCodeBuilder, pContainer);
            pCodeBuilder.AppendLineTop(")");

            AddCodeCreatorsCode(pBuilderContext, pCodeBuilder);
        }

        public CodeCreatorParameter[] GetParameters()
        {
            return new[] { new CodeCreatorParameter() {
                 Direction = VariableDirection.In,
                 Name = "Expression",
                 Type = typeof(CSharpCode)
                }
            };
        }
    }
}
