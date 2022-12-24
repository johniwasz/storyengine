using Microsoft.Extensions.Logging;
using System;

namespace Whetstone.StoryEngine
{
    internal class NullExternalScopeProvider : IExternalScopeProvider

    {

        private NullExternalScopeProvider()

        {

        }



        /// <summary>

        /// Returns a cached instance of <see cref="NullExternalScopeProvider"/>.

        /// </summary>

        public static IExternalScopeProvider Instance { get; } = new NullExternalScopeProvider();



        /// <inheritdoc />

        void IExternalScopeProvider.ForEachScope<TState>(Action<object, TState> callback, TState state)

        {

        }



        /// <inheritdoc />

        IDisposable IExternalScopeProvider.Push(object state)

        {

            return NullScope.Instance;

        }

    }
}
