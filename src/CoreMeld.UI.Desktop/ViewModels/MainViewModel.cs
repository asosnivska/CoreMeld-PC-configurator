using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMeld.UI.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel = new StartViewModel();
        //private ViewModelBase _currentViewModel = new ResultViewModel();
        //private ViewModelBase _currentViewModel = new PartSelectorViewModel();


        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
    }
}
