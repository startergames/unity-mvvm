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
                viewmodel = value;
                OnPropertyChanged();
            }
        }

        public System.Type ViewModelType =>
            Reversal(out _)?.GetType()
         ?? (
                viewmodel is ViewModelRelay relay
                    ? relay.ViewModelType
                    :
                !string.IsNullOrWhiteSpace(relayTypeInfo)
                    ? System.Type.GetType(relayTypeInfo)
                    : null
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

        private ViewModel Reversal(out string prefix, bool ignoreLastPrefix = false) {
            var parent = viewmodel;
            prefix = ignoreLastPrefix ? null : prefixPath;
            while (parent is ViewModelRelay relay) {
                prefix = string.IsNullOrWhiteSpace(prefix)
                             ? relay.prefixPath
                             : prefix.StartsWith('[') 
                                 ? relay.PrefixPath + prefix
                                 : string.Join('.', relay.PrefixPath, prefix);
                if (parent == relay.viewmodel)
                    break;
                
                parent = relay.viewmodel;
            }

            return parent;
        }

        public bool findInParent = false;

        public override async Task Initialize() {
            await viewmodel.InitializeAwaiter();
            viewmodel.PropertyChanged += ViewmodelOnPropertyChanged;
        }

        public override void Finalize() {
            viewmodel.PropertyChanged -= ViewmodelOnPropertyChanged;
        }

        private void ViewmodelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }
    }
}