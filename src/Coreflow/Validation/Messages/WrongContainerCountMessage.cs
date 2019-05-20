﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Coreflow.Validation.Messages
{
    public class WrongContainerCountMessage : IFlowValidationMessage
    {
        public FlowValidationMessageType MessageType => FlowValidationMessageType.WrongCodeCreatorContainerCount;

        public Guid Identifier { get; set; }

        public bool IsFatalError => false;

        public int CurrentCount { get; }

        public int ExpectedCount { get; }

        internal WrongContainerCountMessage(Guid pCodeCreatorIdentifier, int pCurrentCount, int pExpectedCount)
        {
            Identifier = pCodeCreatorIdentifier;
            CurrentCount = pCurrentCount;
            ExpectedCount = pExpectedCount;
        }
    }
}
