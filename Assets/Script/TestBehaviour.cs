using UnityEngine;
using VContainer;

namespace Script {
    public class TestBehaviour : MonoBehaviour {
        [Inject]
        readonly IMessageBoxService _messageBoxService;

        private void Start() {
            _messageBoxService.Show("Test", "TestMessage", "Okay", "No!!!", () => { Debug.Log("Ok clicked"); });
        }
    }
}