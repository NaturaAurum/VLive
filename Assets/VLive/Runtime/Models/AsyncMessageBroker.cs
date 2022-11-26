using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
namespace VLive.Runtime.Models
{
    public class AsyncMessageBroker<T> : IDisposable
    {
        private readonly Channel<T> _channel;
        private readonly IConnectableUniTaskAsyncEnumerable<T> _multicastSource;
        private readonly IDisposable _connection;

        public AsyncMessageBroker()
        {
            _channel = Channel.CreateSingleConsumerUnbounded<T>();
            _multicastSource = _channel.Reader.ReadAllAsync().Publish();
            _connection = _multicastSource.Connect();
        }

        public void Publish(T value)
        {
            _channel.Writer.TryWrite(value);
        }

        public void GetValue(out T value)
        {
            _channel.Reader.TryRead(out value);
        }

        public async Task<T> GetValueAsync(CancellationToken token)
        {
            var value = await _channel.Reader.ReadAsync(token);
            return value;
        }

        public IDisposable Subscribe(Action<T> onNext)
        {
            return _multicastSource.Subscribe(onNext);
        }

        public void Dispose()
        {
            _channel.Writer.TryComplete();
            _connection.Dispose();
        }
    }
}
