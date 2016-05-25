using System;

namespace PostalNET
{
    /// <summary>
    /// The well-known location for the Postal.NET implementation.
    /// </summary>
    public static class Postal
    {
        /// <summary>
        /// The Postal.NET implementation factory.
        /// </summary>
        public static Func<IBox> Factory = () => new Box();

        /// <summary>
        /// Any channel or topic.
        /// </summary>
        public const string All = "*";

        private static IBox _box;

        /// <summary>
        /// The Postal.NET implementation.
        /// </summary>
        public static IBox Box
        {
            get
            {
                if (_box == null)
                {
                    _box = Factory();
                }

                return _box;
            }
        }
    }
}
