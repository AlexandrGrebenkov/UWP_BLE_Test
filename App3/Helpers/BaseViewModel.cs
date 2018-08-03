using System;
using System.Threading.Tasks;

namespace Helpers
{
    public class BaseViewModel : ObservableObject, IDisposable
    {
        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }
        /// <summary>
        /// Private backing field to hold the title
        /// </summary>
        string title = string.Empty;
        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public delegate Task<bool> DisplayAlertFromVM(string title, string message, string ok, string cancel);
        public DisplayAlertFromVM DisplayAlert;

        public delegate Task<string> DisplayActionSheetFromVM(string title, string cancel, string destruction, params string[] buttons);
        public DisplayActionSheetFromVM DisplayActionSheet;

        public bool TimerFlag = true;

        public virtual void Dispose()
        {
            TimerFlag = false;
        }
    }
}
