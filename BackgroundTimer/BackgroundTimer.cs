using Windows.ApplicationModel.Background;
using Windows.UI;

namespace BackgroundTimer
{
    public sealed class Timer : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;

        private async void RefreshColor()
        {
            AccentColor.Helper accentColorHelper = new AccentColor.Helper();
            Color color = await accentColorHelper.GetColor();
            accentColorHelper.SetColor(color);

            deferral.Complete();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            RefreshColor();
        }
    }
}
