using System.Collections.Generic;
using UnityEngine;

public class AttackEffectPool : MonoBehaviour
{
    public static AttackEffectPool Instance;
    public GameObject effectPrefab;
    public int initialSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(CreateNew());
    }

    private GameObject CreateNew()
    {
        GameObject go = Instantiate(effectPrefab, transform);
        go.SetActive(false);
        return go;
    }

    public GameObject Get()
    {
        if (pool.Count == 0) pool.Enqueue(CreateNew());
        var go = pool.Dequeue();
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
