﻿using Coreflow.Helper;
using Coreflow.Interfaces;
using Coreflow.Objects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Coreflow
{
    internal class FlowBuilderHelper
    {
        internal const string INSTANCE_ID_PARAMETER_NAME = "InstanceId";
        internal const string FLOW_NAMESPACE_PREFIX = "FlowNs_";
        internal const string FLOW_CLASS_PREFIX = "Flow_";


        public static IEnumerable<FlowCode> GenerateFlowCode(IEnumerable<FlowDefinition> pFlows)
        {
            return pFlows.Select(w => GenerateFlowCode(w));
        }

        private static void SetParentContainer(ICodeCreator pCodeCreator, ICodeCreatorContainerCreator pParent = null)
        {
            if (pCodeCreator is ICodeCreatorContainerCreator container)
            {
                container.ParentContainerCreator = pParent;

                foreach (var entry in container.CodeCreators)
                {
                    foreach (var cc in entry)
                    {
                        SetParentContainer(cc, container);
                    }
                }
            }
        }

        public static FlowCode GenerateFlowCode(FlowDefinition pFlowDefinition, bool pOnlySingleFlow = false)
        {
            FlowCode ret = new FlowCode();
            ret.Definition = pFlowDefinition;

            SetParentContainer(pFlowDefinition.CodeCreator);

            FlowCodeWriter cw = new FlowCodeWriter();

            FlowBuilderContext context = new FlowBuilderContext(cw, pFlowDefinition);

            string flowid = pFlowDefinition.Identifier.ToString().ToVariableName();

            cw.AppendLineTop("namespace " + FLOW_NAMESPACE_PREFIX + flowid + " {");

            cw.AppendLineBottom("} /* Namespace */"); //Close Namespace

            foreach (string import in pFlowDefinition.ReferencedNamespaces.Distinct())
            {
                cw.AppendLineTop($"using {import};");
            }

            cw.AppendLineTop();

            cw.WriteIdentifierTagTop(pFlowDefinition);
            cw.WriteContainerTagTop(pFlowDefinition);

            cw.AppendLineTop($"[Coreflow.Objects.FlowIdentifierAttribute(\"{pFlowDefinition.Identifier.ToString()}\")]");

            //Currently idk which letters needs an escape
            cw.AppendLineTop("public class " + FLOW_CLASS_PREFIX + flowid + " : " + typeof(ICompiledFlow).FullName + "  {");


            cw.AppendLineTop();

            cw.AppendLineBottom("} /* Class */"); //Close Class


            cw.AppendLineTop($"public Guid {INSTANCE_ID_PARAMETER_NAME} = Guid.NewGuid();");

            cw.AppendLineTop();

            if (pFlowDefinition.Arguments != null)
                foreach (FlowArguments parameter in pFlowDefinition.Arguments)
                {
                    string value = $"default({parameter.Type.FullName})";

                    if (parameter.Expression != null && !string.IsNullOrWhiteSpace(parameter.Expression))
                        value = parameter.Expression;

                    cw.AppendLineTop($"public {parameter.Type.FullName} {parameter.Name} = {value};");
                }


            cw.AppendLineTop();


            cw.AppendLineTop("public void SetArguments(IDictionary<string, object> pArguments) {");

            if (pFlowDefinition.Arguments != null)
                foreach (FlowArguments arg in pFlowDefinition.Arguments)
                {
                    cw.AppendLineTop("if (pArguments.ContainsKey(\"" + arg.Name + "\"))  { " + arg.Name + " = (" + arg.Type.FullName + ")pArguments[\"" + arg.Name + "\"];  }");
                }

            cw.AppendLineTop("}");

            cw.AppendLineTop();


            cw.AppendLineTop("public IDictionary<string, object> GetArguments() {");
            cw.AppendLineTop("Dictionary<string, object> ret = new Dictionary<string, object>();");

            if (pFlowDefinition.Arguments != null)
                foreach (FlowArguments arg in pFlowDefinition.Arguments)
                {
                    cw.AppendLineTop("ret.Add(\"" + arg.Name + "\", " + arg.Name + ");");
                }

            cw.AppendLineTop("return ret;");
            cw.AppendLineTop("}");

            cw.AppendLineTop();

            cw.AppendLineTop("Guid Coreflow.Interfaces.ICompiledFlow.InstanceId => InstanceId;");

            cw.AppendLineTop();

            cw.AppendLineTop("public void Run() { ");

            cw.AppendLineBottom("} /* Run */"); //Close Run


            if (pFlowDefinition.CodeCreator != null)
            {
                if (pFlowDefinition.CodeCreator is IVariableCreator ccv)
                    ccv.Initialize(context, cw);

                pFlowDefinition.CodeCreator.ToCode(context, cw);
            }

            ret.Code = cw.ToString();
            return ret;
        }

        public static IVariableCreator GetVariableCreatorInInitialScope(ICodeCreatorContainerCreator pContainer, ICodeCreator pCodeCreator, Func<IVariableCreator, bool> pFilter)
        {
            if (pContainer == null)
                return null;

            foreach (var cclist in pContainer.CodeCreators)
            {
                IVariableCreator found = cclist.Select(v => v as IVariableCreator).Where(v => v != null).FirstOrDefault(pFilter);
                if (found != null)
                    return found;
            }

            return GetVariableCreatorInInitialScope(pContainer.ParentContainerCreator, pContainer, pFilter);
        }

        /*
        public static FlowCode GetCombinedCodeOtherFlows(FlowDefinition pFlowDefinition)
        {

            StringBuilder combinedCode = new StringBuilder();

            foreach (var flow in pFlowDefinition.Coreflow.FlowDefinitionStorage.GetDefinitions())
            {
                if (flow.Identifier == pFlowDefinition.Identifier)
                    continue;

                FlowCode fcode = FlowBuilderHelper.GenerateFlowCode(flow, true);

                combinedCode.Append(fcode.Code);
            }

            string fullcode = combinedCode.ToString();

            return new FlowCode()
            {
                Code = fullcode
            };
        }*/

        public static string FormatCode(string pCode)
        {
            return FormatCode(FlowCompilerHelper.ParseText(pCode, false));
        }

        private static string FormatCode(SyntaxTree pSyntaxTree)
        {
            var workspace = new AdhocWorkspace();

            SyntaxNode formattedNode = Formatter.Format(pSyntaxTree.GetRoot(), workspace);

            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                formattedNode.WriteTo(writer);
            }

            return sb.ToString();
        }
    }
}
