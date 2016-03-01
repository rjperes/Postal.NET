using Postal.NET;

namespace PostalConventions.NET
{
    public static class ConventionsBoxExtensions
    {
        public static IConventionsBox WithConventions(this IBox box)
        {
            return new ConventionsBox(box);
        }
    }
}
