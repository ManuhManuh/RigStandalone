using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = (T)this;
        OnAwake();
    }

    /// <summary>
    ///     Called on MonoBehaviour Awake after singleton setup logic
    /// </summary>
    protected virtual void OnAwake()
    {
    }

}
