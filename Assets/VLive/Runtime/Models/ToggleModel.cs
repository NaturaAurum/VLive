using UniRx;
namespace VLive.Runtime.Models
{
    public class ToggleModel
    {
        public IReadOnlyReactiveProperty<bool> Toggle => _toggleRx;
        private readonly BoolReactiveProperty _toggleRx;

        public ToggleModel()
        {
            _toggleRx = new BoolReactiveProperty(false);
        }

        public void Trigger()
        {
            _toggleRx.Value = !_toggleRx.Value;
        }
    }
}
