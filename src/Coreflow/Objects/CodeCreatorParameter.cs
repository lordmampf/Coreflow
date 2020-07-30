﻿using System;

namespace Coreflow.Objects
{
    public class CodeCreatorParameter
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Type Type { get; set; }

        public string Category { get; set; } = "Default";

        public VariableDirection Direction { get; set; }

        public string DefaultValueCode { get; set; }

        public CodeCreatorParameter()
        {
        }

        public CodeCreatorParameter(string pName, string pDisplayName, Type pType, string pCategory, VariableDirection pDirection, string pDefaultValueCode)
        {
            Name = pName;
            DisplayName = pDisplayName;
            Type = pType;
            Category = pCategory;
            Direction = pDirection;
            DefaultValueCode = pDefaultValueCode;
        }

        public string GetDisplayNameOrName()
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
                return DisplayName;
            return Name;
        }
    }
}
