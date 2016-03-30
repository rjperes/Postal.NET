namespace Postal.NET
{
    /// <summary>
    /// The well-known location for the Postal.NET implementation.
    /// </summary>
    public static class Postal
    {
        /// <summary>
        /// Any channel or topic.
        /// </summary>
        public const string All = "*";

        /// <summary>
        /// The Postal.NET implementation.
        /// </summary>
        public static readonly IBox Box = new Box();
    }
}
