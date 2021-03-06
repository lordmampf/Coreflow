﻿using Coreflow.Helper;
using Coreflow.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coreflow.Objects
{
    public class FlowBuilderContext
    {
        private Dictionary<Guid, string> mLocalObjectNames = new Dictionary<Guid, string>();

        public Dictionary<string, object> BuildingContext = new Dictionary<string, object>();

        private FlowCodeWriter mCodeWriter;

        public IEnumerable<ISymbol> CurrentSymbols { get; protected set; }

        public SemanticModel SemanticModel { get; protected set; }

        public SyntaxTree SyntaxTree { get; protected set; }

        public FlowDefinition FlowDefinition { get; protected set; }

        public Compilation Compilation { get; protected set; }


        public FlowBuilderContext(FlowCodeWriter pCodeWriter, FlowDefinition pCurrentDefinition)
        {
            mCodeWriter = pCodeWriter;
            FlowDefinition = pCurrentDefinition;
        }

        public string GetOrCreateLocalVariableName(IVariableCreator pVariableCreator)
        {
            if (mLocalObjectNames.ContainsKey(pVariableCreator.Identifier))
                return mLocalObjectNames[pVariableCreator.Identifier];

            string variableName = Guid.NewGuid().ToString().ToVariableName();

            mLocalObjectNames.Add(pVariableCreator.Identifier, variableName);
            return variableName;
        }

        public string CreateLocalVariableName(IVariableCreator pVariableCreator)
        {
            string variableName = pVariableCreator.Identifier.ToString().ToVariableName();
            mLocalObjectNames.Add(pVariableCreator.Identifier, variableName);
            return variableName;
        }

        public void SetLocalVariableName(IVariableCreator pVariableCreator, string pLocalVariableName)
        {
            mLocalObjectNames.Add(pVariableCreator.Identifier, pLocalVariableName);
        }

        public string GetLocalVariableName(IVariableCreator pVariableCreator)
        {
            return mLocalObjectNames[pVariableCreator.Identifier];
        }

        public bool HasLocalVariableName(IVariableCreator pVariableCreator)
        {
            return mLocalObjectNames.ContainsKey(pVariableCreator.Identifier);
        }

        public void UpdateCurrentSymbols()
        {
            string code = mCodeWriter.ToString();

            SyntaxTree = FlowCompilerHelper.ParseCode(code);

            SyntaxNode sn = GetCursorNode(SyntaxTree);

            CurrentSymbols = CompilationLookUpSymbols(SyntaxTree, sn as CSharpSyntaxNode);
        }

        protected SyntaxNode GetCursorNode(SyntaxTree tree)
        {
            var cursorLine = tree.GetText().Lines.FirstOrDefault(l => l.ToString() == FlowCodeWriter.CURSOR_LOC);

            var lineSpan = cursorLine.Span;

            return tree.GetRoot().FindNode(lineSpan);
        }

        protected IEnumerable<ISymbol> CompilationLookUpSymbols(SyntaxTree tree, CSharpSyntaxNode currentNode)
        {
            Compilation = FlowCompilerHelper.CreateCompilation("dummy", tree);

            var diagnostics = Compilation.GetDiagnostics();

            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

            if (errors.Any())
            {
                //TODO Now it is possible that we can not resolve a type of a symbol. We hope the user will correct that error in code asap!
                //It's possible that we call another flow and we don't know the type here. But this can be ignored right now
                //Add a warning or something?
                //Throw exception?

                Console.WriteLine($"WARNING: Errors in {nameof(CompilationLookUpSymbols)}. This could be an bug in a code creator or just a call to another flow");

                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }

            SemanticModel = Compilation.GetSemanticModel(tree);
            return SemanticModel.LookupSymbols(currentNode.SpanStart);
        }

    }
}
