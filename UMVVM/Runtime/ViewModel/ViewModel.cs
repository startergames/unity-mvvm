using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Starter.ViewModel {
    public abstract class ViewModel : MonoBehaviour, INotifyPropertyChanged {
        private readonly TaskCompletionSource<bool> _initializeTaskCompletionSource = new();

        public bool IsInitialized {
            get => _initializeTaskCompletionSource.Task.IsCompleted && _initializeTaskCompletionSource.Task.Result;
            private set => _initializeTaskCompletionSource.SetResult(value);
        }

        public abstract Task Initialize();
        public abstract void Finalize();

        public async Task InitializeAwaiter() {
            await _initializeTaskCompletionSource.Task;
        }

        private async void Start() {
            await Initialize();
            IsInitialized = true;
        }

        private void OnDestroy() {
            Finalize();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}