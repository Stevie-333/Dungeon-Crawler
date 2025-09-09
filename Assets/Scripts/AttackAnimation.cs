using UnityEngine;

public class EffectAutoReturn : MonoBehaviour
{
    public float lifeTime = 0.1f; // set to match effect animation length

    void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    void ReturnToPool()
    {
        if (AttackEffectPool.Instance != null)
            AttackEffectPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }
}
