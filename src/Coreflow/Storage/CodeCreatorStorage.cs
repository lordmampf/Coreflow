﻿using Coreflow.CodeCreators;
using Coreflow.Helper;
using Coreflow.Interfaces;
using Coreflow.Objects;
using Coreflow.Objects.CodeCreatorFactory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coreflow.Storage
{
    public class CodeCreatorStorage
    {
        private readonly Dictionary<string, ICodeCreatorFactory> mCustomCodeCCreatorFactories = new Dictionary<string, ICodeCreatorFactory>();

        private readonly Dictionary<Type, ICodeCreatorFactory> mDefaultConstructorFactories = new Dictionary<Type, ICodeCreatorFactory>();

        private Coreflow mCoreflow;

        internal CodeCreatorStorage(Coreflow pCoreflow)
        {
            mCoreflow = pCoreflow;
        }

        public void AddCodeCreatorFactory(ICodeCreatorFactory pFactory)
        {
            mCustomCodeCCreatorFactories.Add(pFactory.Identifier, pFactory);
        }

        public void AddCodeCreatorDefaultConstructor(Type pCodeCreatorType)
        {
            if (!typeof(ICodeCreator).IsAssignableFrom(pCodeCreatorType))
                throw new ArgumentException($"type {pCodeCreatorType.FullName} does not implement {nameof(ICodeCreator)}");

            DefaultConstructorCodeCreatorFactory factory = new DefaultConstructorCodeCreatorFactory(pCodeCreatorType);
            mDefaultConstructorFactories.Add(pCodeCreatorType, factory);
        }

        public void AddCodeCreatorDefaultConstructor(IEnumerable<Type> pCodeCreatorType)
        {
            pCodeCreatorType.ForEach(t => AddCodeCreatorDefaultConstructor(t));
        }

        public void AddCodeActivity(IEnumerable<Type> pCodeActivityTypes)
        {
            pCodeActivityTypes.ForEach(t => AddCodeActivity(t));
        }

        public void AddCodeActivity(Type pCodeActivityType)
        {
            Type generic = typeof(CodeActivityCreator<>).MakeGenericType(pCodeActivityType);
            AddCodeCreatorDefaultConstructor(generic);
        }

        public IEnumerable<ICodeCreatorFactory> GetAllFactories()
        {
            return mCustomCodeCCreatorFactories.Values.Concat(mDefaultConstructorFactories.Values);
        }

        public ICodeCreatorFactory GetFactory(string pType, string pFactoryIdentifier)
        {
            Type type = TypeHelper.SearchType(pType);

            if (mDefaultConstructorFactories.ContainsKey(type))
                return mDefaultConstructorFactories[type];

            if (pFactoryIdentifier != null && mCustomCodeCCreatorFactories.ContainsKey(pFactoryIdentifier))
                return mCustomCodeCCreatorFactories[pFactoryIdentifier];

            return new CodeCreatorMissingFactory(pType, pFactoryIdentifier);
        }

        public void RemoveFactory(string pFactoryIdentifier)
        {
            mCustomCodeCCreatorFactories.Remove(pFactoryIdentifier);
        }
    }
}
