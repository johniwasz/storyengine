using System;

namespace Whetstone.StoryEngine
{
    internal class NullScope : IDisposable

    {

        public static NullScope Instance { get; } = new NullScope();



        private NullScope()

        {

        }



        /// <inheritdoc />

        public void Dispose()

        {

        }

    }
}
