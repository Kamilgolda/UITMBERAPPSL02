﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using UITMBER.Models;
using UITMBER.Services;
using UITMBER.Services.Authentication;
using System.Windows.Input;
using System.Threading.Tasks;

namespace UITMBER.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {

        public IAuthenticationService AuthService => DependencyService.Get<IAuthenticationService>();

        public ICommand LogOutCommand => new Command(async () => await OnLogutAsync());

        private async Task OnLogutAsync()
        {
            Settings.AccessToken = "";
            Settings.Name = "";
            Settings.Photo = "";
            Settings.Roles = "";
            Settings.TokenExpire = DateTime.MinValue;

            await Shell.Current.GoToAsync($"//{nameof(Views.LoginPage)}");
        }

        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
