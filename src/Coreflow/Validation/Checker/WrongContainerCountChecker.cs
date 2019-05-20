﻿using System;
using System.Collections.Generic;
using System.Text;
using Coreflow.Interfaces;
using Coreflow.Validation.Messages;

namespace Coreflow.Validation.Checker
{
    internal class WrongContainerCountChecker : ICodeCreatorChecker
    {
        public void Check(ref List<IFlowValidationMessage> pMessages, ICodeCreator pCodeCreator)
        {
            if (pCodeCreator is ICodeCreatorContainerCreator container)
            {
                if (container.SequenceCount != container.CodeCreators.Count)
                {
                    pMessages.Add(new WrongContainerCountMessage(container.Identifier, container.CodeCreators.Count, container.SequenceCount));
                }
            }
        }
    }
}