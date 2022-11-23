using VLive.Runtime.Models;
namespace VLive.Runtime.Presenters
{
    public class PointToggleButtonPresenter : ToggleButtonPresenter
    {
        protected override ToggleModel Toggle => StaticModels.Instance.PointToggle;
    }
}
