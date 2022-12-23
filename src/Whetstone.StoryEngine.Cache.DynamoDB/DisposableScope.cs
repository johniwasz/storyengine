﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Cache.DynamoDB
{
    public sealed class DisposableScope : IDisposable
    {
        private readonly Action _closeScopeAction;
        public DisposableScope(Action closeScopeAction)
        {
            _closeScopeAction = closeScopeAction;
        }
        public void Dispose()
        {
            _closeScopeAction();
        }
    }
}
