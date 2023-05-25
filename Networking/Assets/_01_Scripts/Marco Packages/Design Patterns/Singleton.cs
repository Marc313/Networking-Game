using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
        set
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }

            instance = value;
        }
    }
}
