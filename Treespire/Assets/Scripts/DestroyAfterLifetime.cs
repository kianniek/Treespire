using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterLifetime : MonoBehaviour
{
    [SerializeField] private float lifeTime;

    /// <summary>
    /// Update is called once per frame. 
    /// </summary>
    private void Update()
    {
        // decrease lifetime
        lifeTime -= Time.unscaledDeltaTime;

        // until we need to destroy ourselves...
        if (lifeTime <= 0)
            Destroy(gameObject);
    }
}
