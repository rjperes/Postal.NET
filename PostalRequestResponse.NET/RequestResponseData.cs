using System;

namespace PostalRequestResponse.NET
{
    public interface IRequestResponseData
    {
        Guid CorrelationId { get; }
        object Data { get; }
    }

    sealed class RequestResponseData : IRequestResponseData
    {
        public RequestResponseData(object data, Guid correlationId)
        {
            this.CorrelationId = correlationId;
            this.Data = data;
        }

        public Guid CorrelationId { get; private set; }

        public object Data { get; private set; }
    }
}
