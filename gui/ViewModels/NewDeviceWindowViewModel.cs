using Caliburn.Micro;
using gui.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gui.ViewModels
{
    public class NewDeviceWindowViewModel : Screen
    {
        #region Properties
        private string _nodeDisplayName = "";
        private string _azureConnectionString = "";
        private bool _isDeviceSubmitting = false;

        public string NodeDisplayName
        {
            get { return _nodeDisplayName; }
            set { _nodeDisplayName = value; NotifyOfPropertyChange(() => NodeDisplayName); }
        }

        public string AzureConnectionString
        {
            get { return _azureConnectionString; }
            set { _azureConnectionString = value; NotifyOfPropertyChange(() => AzureConnectionString); }
        }

        public bool IsDeviceSubmitting
        {
            get { return _isDeviceSubmitting; }
            set { 
                _isDeviceSubmitting = value;
                NotifyOfPropertyChange(() => IsDeviceSubmitting);
                NotifyOfPropertyChange(() => SubmittingTextBlockVisibility);
                NotifyOfPropertyChange(() => SubmitButtonVisibility);
            }
        }

        public Visibility SubmittingTextBlockVisibility { get { return IsDeviceSubmitting ? Visibility.Visible : Visibility.Hidden; } }
        public Visibility SubmitButtonVisibility { get { return IsDeviceSubmitting ? Visibility.Hidden : Visibility.Visible; } }
        #endregion


        #region Constructor
        public NewDeviceWindowViewModel()
        {
                
        }
        #endregion


        #region Methods
        private async void Submit()
        {
            IsDeviceSubmitting = true;
            await Task.Delay(2000);
            Exception? res = await Globals.IoTAgent.AddNewDevice(NodeDisplayName, AzureConnectionString);
            if (res != null)
            {
                MessageBox.Show(res.Message);
                IsDeviceSubmitting = false;
            }
            else
                await this.TryCloseAsync();
        }
        #endregion


        #region Button clicks
        public void SubmitButton()
        {
            Submit();
        }
        #endregion
    }
}
