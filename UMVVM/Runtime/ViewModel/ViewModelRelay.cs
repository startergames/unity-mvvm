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

        public ViewModel ViewModel => Reversal(out _);

        public System.Type ViewModelType =>
            Reversal(out _)?.GetType()
         ?? (
                !string.IsNullOrWhiteSpace(relayTypeInfo)
                    ? System.Type.GetType(relayTypeInfo)
                    : null
            );

        public string PrefixPath {
            get {
                Reversal(out var prefix);
                return prefix;
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
                             : string.Join('.', relay.prefixPath, prefix);
                parent = relay.viewmodel;
            }

            return parent;
        }

        public bool findInParent = false;

        public override async Task Initialize() {
            await viewmodel.InitializeAwaiter();
        }
    }
}