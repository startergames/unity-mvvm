using System;
using UnityEngine;

namespace Attributes {
    public class ViewModelPathAttribute : PropertyAttribute {
        public Type   type   { get; }
        public string member { get; }

        public ViewModelPathAttribute(string member = null, Type type = null) {
            this.type   = type;
            this.member = member;
        }
    }
}