﻿using Coreflow.Objects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Coreflow.Helper
{
    internal static class WorkflowCompilerHelper
    {
        public const string COMMENT_ID_PREFIX = "//#id ";
        public const string CONTAINER_ID_PREFIX = "//#Container";

        private static Regex mIdRegex = new Regex(@"\/\/#id *([a-zA-Z0-9-]*)");
        private static Regex mContainerRegex = new Regex(@"\/\/#Container");

        public static WorkflowCompileResult CompileWorkflowCode(IEnumerable<WorkflowCode> pWorkflowCode, string pAssemblyName = "DynamicCreatedWorkflowsAssembly")
        {
            return CompileWorkflowCode(new WorkflowCode
            {
                Code = pWorkflowCode.Select(w => w.Code).Aggregate((o, n) => o + Environment.NewLine + n),
                ReferencedAssemblies = pWorkflowCode.SelectMany(w => w.ReferencedAssemblies).Distinct()
            }, pAssemblyName);
        }

        public static WorkflowCompileResult CompileWorkflowCode(WorkflowCode pWorkflowCode, string pAssemblyName = "DynamicCreatedWorkflowsAssembly")
        {
            WorkflowCompileResult ret = new WorkflowCompileResult()
            {
                WorkflowCode = pWorkflowCode
            };

            SyntaxTree syntaxTree = ParseText(pWorkflowCode.Code);

            //  string formattedCode = FormatCode(syntaxTree);        

            Compilation compilation = CreateLibraryCompilation(pAssemblyName, false)
               .AddReferences(pWorkflowCode.ReferencedAssemblies)
               .AddSyntaxTrees(syntaxTree);

            var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            string[] codeLines = pWorkflowCode.Code.Split("\n");

            if (!emitResult.Success)
            {
                IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(
                    diagnostic => diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                string errors = "";

                ret.ErrorCodeCreators = new Dictionary<Guid, string>();

                foreach (Diagnostic diagnostic in failures)
                {
                    int line = diagnostic.Location.GetLineSpan().StartLinePosition.Line;

                    Guid ccGuid = GetIdentifier(codeLines, line);

                    if (!ret.ErrorCodeCreators.ContainsKey(ccGuid))
                    {
                        ret.ErrorCodeCreators.Add(ccGuid, diagnostic.GetMessage());
                    }
                    else
                    {
                        ret.ErrorCodeCreators[ccGuid] += Environment.NewLine + diagnostic.GetMessage();
                    }

                    errors += diagnostic.GetMessage();
                    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }

                ret.ErrorMessage = errors;

                return ret;
            }


            /*
            Dictionary<Guid, List<string>> containerVariables = new Dictionary<Guid, List<string>>();

            void addToContainerVariables(Guid pContainerGuid, string pVariableIdentifier)
            {
                if (pVariableIdentifier.IsContainerCreatorVariableName())
                    return;

                if (containerVariables.ContainsKey(pContainerGuid))
                    containerVariables[pContainerGuid].Add(pVariableIdentifier);
                else
                {
                    containerVariables.Add(pContainerGuid, new List<string>());
                    containerVariables[pContainerGuid].Add(pVariableIdentifier);
                }
            };

            var root = (CompilationUnitSyntax)syntaxTree.GetRoot();

            var variableDeclarations = root.DescendantNodes().OfType<VariableDeclarationSyntax>();

            foreach (var variableDeclaration in variableDeclarations)
            {
                string identifier = variableDeclaration.Variables.First().Identifier.Value.ToString();

                int line = variableDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line;
                int containerLine = GetContainerLineOfCode(codeLines, line);
                Guid containerGuid = GetIdentifier(codeLines, containerLine);

                addToContainerVariables(containerGuid, identifier);
            }

            var variableDeclarationsParams = root.DescendantNodes().OfType<SingleVariableDesignationSyntax>();

            foreach (var variableDeclaration in variableDeclarationsParams)
            {
                string identifier = variableDeclaration.Identifier.Value.ToString();

                int line = variableDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line;
                int containerLine = GetContainerLineOfCode(codeLines, line);
                Guid containerGuid = GetIdentifier(codeLines, containerLine);

                addToContainerVariables(containerGuid, identifier);
            }

    */

            ret.Successful = true;
            ret.ResultAssembly = Assembly.Load(stream.ToArray());

            return ret;
        }

        public static SyntaxTree ParseText(string sourceCode)
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: LanguageVersion.Latest);

            // Return a syntax tree of our source code
            return CSharpSyntaxTree.ParseText(sourceCode, options);
        }

        private static Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
        {
            var options = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: enableOptimisations ? OptimizationLevel.Release : OptimizationLevel.Debug,
                    allowUnsafe: true);

            return CSharpCompilation.Create(assemblyName, options: options);
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

        private static Guid GetIdentifier(string[] pCode, int pLineOfCode)
        {
            for (pLineOfCode--; pLineOfCode > 0; pLineOfCode--)
            {
                var regexResult = mIdRegex.Match(pCode[pLineOfCode]);
                if (regexResult.Success)
                {
                    return Guid.Parse(regexResult.Groups[1].Value);
                }
            }

            throw new Exception("GetIdentifier: Identifier not found!");
        }

        internal static int GetLineOfCode(string[] pCode, Guid pIdentifier)
        {
            for (int i = 0; i < pCode.Length; i++)
            {
                var regexResult = mIdRegex.Match(pCode[i]);
                if (regexResult.Success)
                {
                    return i;
                }
            }

            throw new Exception("GetLineOfCode: Identifier not found!");
        }

        private static int GetContainerLineOfCode(string[] pCode, int pLineOfCode)
        {
            for (pLineOfCode--; pLineOfCode > 0; pLineOfCode--)
            {
                var regexResult = mContainerRegex.Match(pCode[pLineOfCode]);
                if (regexResult.Success)
                {
                    return pLineOfCode;
                }
            }

            throw new Exception("GetContainer: Container not found!");
        }

    }
}
