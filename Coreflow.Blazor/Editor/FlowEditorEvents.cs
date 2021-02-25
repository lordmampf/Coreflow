﻿using Coreflow.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Coreflow.Blazor.Editor
{
    public class FlowEditorEvents : ComponentBase
    {
        [Parameter]
        public EventCallback<ICodeCreator> OnAfterSelect { get; set; }

        private FlowEditor mFlowEditor;

        [CascadingParameter]
        public FlowEditor FlowEditor
        {
            get
            {
                return mFlowEditor;
            }
            set
            {
                if (value != mFlowEditor)
                {
                    mFlowEditor = value;
                    mFlowEditor.RegisterEvents(this);
                }
            }
        }
    }
}
