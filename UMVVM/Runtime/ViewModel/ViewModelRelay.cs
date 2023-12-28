using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Attributes;
using UnityEngine;

namespace Starter.ViewModel {
    public class ViewModelRelay : ViewModel {
        [SerializeField]
        private ViewModel viewmodel;

        [SerializeField]
        private string relayTypeInfo;

        [SerializeField]
        [ViewModelPath]
        private string prefixPath;

        public ViewModel ViewModel {
            get => Reversal(out _);
            set {
                if (viewmodel != null)
                    viewmodel.PropertyChanged -= ViewmodelOnPropertyChanged;
                viewmodel = value;
                if (viewmodel != null)
                    viewmodel.PropertyChanged += ViewmodelOnPropertyChanged;
                OnPropertyChanged();
            }
        }

        public System.Type ViewModelType =>
            Reversal(out _)?.GetType()
         ?? (
                viewmodel is ViewModelRelay relay         ? relay.ViewModelType :
                !string.IsNullOrWhiteSpace(relayTypeInfo) ? System.Type.GetType(relayTypeInfo) : null
            );

        public string PrefixPath {
            get {
                Reversal(out var prefix);
                return prefix;
            }
            set {
                prefixPath = value;
                OnPropertyChanged();
            }
        }

        public string PrefixPathExceptLast {
            get {
                Reversal(out var prefix, true);
                return prefix;
            }
        }

        private void Awake() {
            RegistPropertyChangedCallback();
        }

        private void RegistPropertyChangedCallback() {
            if (viewmodel == null)
                return;
            viewmodel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }

        private ViewModel Reversal(out string prefix, bool ignoreLastPrefix = false) {
            var parent = viewmodel;
            prefix = ignoreLastPrefix ? null : prefixPath;
            while (parent is ViewModelRelay relay) {
                var parentPrefix = relay.PrefixPath;

                prefix = string.IsNullOrWhiteSpace(prefix)       ? relay.prefixPath :
                         prefix.StartsWith('[')                  ? parentPrefix + prefix :
                         string.IsNullOrWhiteSpace(parentPrefix) ? prefix : string.Join('.', parentPrefix, prefix);

                if (parent == relay.viewmodel)
                    break;

                parent = relay.viewmodel;
            }

            return parent;
        }

        public bool findInParent = false;

        public override async Task Initialize() {
            if (viewmodel == null)
                return;

            await viewmodel.InitializeAwaiter();
            viewmodel.PropertyChanged += ViewmodelOnPropertyChanged;
        }

        public override void Finalize() {
            if (viewmodel == null)
                return;
            viewmodel.PropertyChanged -= ViewmodelOnPropertyChanged;
        }

        private void ViewmodelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }
    }
}