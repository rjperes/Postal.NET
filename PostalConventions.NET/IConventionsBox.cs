using System;
using System.Threading.Tasks;

namespace PostalConventionsNET
{
    public interface IConventionsBox
    {
        void Publish<T>(T data);
        Task PublishAsync<T>(T data);

        IDisposable Subscribe<T>(Action<T> subscriber);

        IConventionsBox AddChannelConvention<T>(Func<T, string> convention);
        IConventionsBox AddTopicConvention<T>(Func<T, string> convention);
    }
}
