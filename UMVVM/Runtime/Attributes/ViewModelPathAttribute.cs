using System;
using UnityEngine;

namespace Attributes {
    public class ViewModelPathAttribute : PropertyAttribute {
        private int depth;
        private Type type;
        public ViewModelPathAttribute(int depth = 0, Type type = null) {
            this.depth = depth;
            this.type  = type;
        }
    }
}