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
        #endregion


        #region Constructor
        public NewDeviceWindowViewModel()
        {
                
        }
        #endregion


        #region Methods
        private void Submit()
        {
            Exception? res = Globals.IoTAgent.AddNewDevice(NodeDisplayName, AzureConnectionString);
            if (res != null) MessageBox.Show(res.Message);
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
