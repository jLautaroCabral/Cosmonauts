using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAxis : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] Vector3 axis = Vector3.forward;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(Rotate));
    }

    IEnumerator Rotate()
    {
        while(true)
        {
            transform.Rotate(axis * speed * Time.deltaTime);
            yield return new WaitForSeconds(0.02f);
        }
    }
}
