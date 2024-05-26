
public static class Utilities {
    /// <summary>
    /// run the given anonymous function next frame
    /// </summary>
    /// <param name="callback">anonymous function to be executed next frame</param>
    public static void WaitOneFrame(Action callback) {
        MonoBehaviourHelper.Instance.StartCoroutine(Coroutine(callback));
        static IEnumerator Coroutine(Action callback) {
            yield return null;
            callback();
        }
    }
    /// <summary>
    /// helper singleton to be able to access monobehavior methods from Utilities.
    /// </summary>
    private class MonoBehaviourHelper : MonoBehaviour {
        private static MonoBehaviourHelper _instance;
        public static MonoBehaviourHelper Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<MonoBehaviourHelper>();
                    if (_instance == null) {
                        GameObject go = new("MonoBehaviourHelper");
                        _instance = go.AddComponent<MonoBehaviourHelper>();
                    }
                }
                return _instance;
            }
        }
    }
}