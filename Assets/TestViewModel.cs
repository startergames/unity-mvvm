using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Starter.ViewModel {
    [System.Serializable]
    public class Temp {
        public double       TestDouble { get; set; } = 1000;
        public int[]        TestArray  { get; set; } = { 1, 2, 3, 4, 5 };
        public List<string> TestList   { get; set; } = new List<string> { "one", "two", "three", "four", "five" };

        public Dictionary<int, string> TestDict { get; set; } = new Dictionary<int, string> {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
            { 4, "four" },
            { 5, "five" }
        };

        public Sprite[] TestSprite;
    }

    public class TestViewModel : ViewModel {
        public override async Task Initialize() {
            TestString = "TestString";
            TestFloat  = 10.0f;
            Temp       = new Temp();
        }

        public override void Finalize() { }

        public string   TestString;
        public float    TestFloat;
        public Temp     Temp;
        public Sprite[] TestSprite;

        public void SetStringAndFloat(string s, float f) {
            SetField(ref TestString, s, nameof(TestString));
            SetField(ref TestFloat, f, nameof(TestFloat));
        }
    }
}