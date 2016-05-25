using System;

namespace PostalNET
{
    /// <summary>
    /// The well-known location for the Postal.NET implementation.
    /// </summary>
    public static class Postal
    {
        private static readonly Func<IBox> _defaultFactory = () => new Box();
        private static Func<IBox> _factory = _defaultFactory;
        private static IBox _box;

        /// <summary>
        /// The Postal.NET implementation factory.
        /// </summary>
        public static Func<IBox> Factory
        {
            get
            {
                return _factory;
            }
            set
            {
                _factory = value ?? _defaultFactory;
            }
        }

        /// <summary>
        /// Any channel or topic.
        /// </summary>
        public const string All = "*";

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
