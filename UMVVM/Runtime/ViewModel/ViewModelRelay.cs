using System.Threading.Tasks;
using Attributes;
using UnityEngine;

namespace Starter.ViewModel {
    public class ViewModelRelay : ViewModel {
        [SerializeField]
        private ViewModel viewmodel;

        [SerializeField]
        [ViewModelPath]
        private string prefixPath;

        public ViewModel ViewModel => Reversal(out _);

        public string PrefixPath {
            get {
                Reversal(out var prefix);
                return prefix;
            }
        }

        public string PrefixPathExceptLast {
            get {
                var    parent = viewmodel;
                string prefix = null;
                while (parent is ViewModelRelay relay) {
                    prefix = string.IsNullOrWhiteSpace(prefix)
                                 ? relay.prefixPath
                                 : string.Join('.', relay.prefixPath, prefix);
                    parent = relay.viewmodel;
                }

                return prefix;
            }
        }

        private ViewModel Reversal(out string prefix) {
            var parent = viewmodel;
            prefix = prefixPath;
            while (parent is ViewModelRelay relay) {
                prefix = relay.prefixPath + "." + prefix;
                parent = relay.viewmodel;
            }

            return parent;
        }

        public bool   findInParent = false;
        public string typeName;

        public override async Task Initialize() {
            await viewmodel.InitializeAwaiter();
        }
    }
}