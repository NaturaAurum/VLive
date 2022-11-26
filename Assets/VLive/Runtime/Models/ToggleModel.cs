using System;
namespace VLive.Runtime.Models
{
    public class ToggleModel
    {
        public bool Value { get; private set; }
        private readonly AsyncMessageBroker<bool> _toggleRx;

        public ToggleModel()
        {
            _toggleRx = new AsyncMessageBroker<bool>();
        }

        public void Toggle()
        {
            Value = !Value;
            _toggleRx.Publish(Value);
        }

        public IDisposable Subscribe(Action<bool> onNext)
        {
            return _toggleRx.Subscribe(onNext);
        }
    }
}
