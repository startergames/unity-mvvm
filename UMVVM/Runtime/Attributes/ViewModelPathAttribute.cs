using UnityEngine;

namespace Attributes {
    public class ViewModelPathAttribute : PropertyAttribute {
        private int depth;
        public ViewModelPathAttribute(int depth = 0) {
            this.depth = depth;
        }
    }
}