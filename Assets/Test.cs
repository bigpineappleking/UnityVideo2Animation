using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    public AnimationClip clip;
    public Transform targetTransform;
    public Transform parent;
    public Transform root;
    void Start()
    {
        //transform.localEulerAngles
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("Run up")]
    public void TestHumanoid1()
    {
        Vector3 targetDir = transform.position - targetTransform.position;
        //transform.rotation = Quaternion.LookRotation(targetDir, Vector3.forward) * Quaternion.Euler(-90, 0, 0);
        Quaternion rotation = Quaternion.LookRotation(targetDir, Vector3.forward) * Quaternion.Euler(-90, 0, 0);
        transform.rotation = rotation;
        Debug.Log(rotation.eulerAngles);

        //new Vector3(90, 90, 90);
    }

    [ContextMenu("Calculate")]
    Vector3 CalculateLocalEuler()
    {
        Quaternion parentWorldRotation = Quaternion.Euler(parent.eulerAngles);
        Quaternion childWorldRotation = Quaternion.Euler(transform.eulerAngles);

        Quaternion childLocalRotation = Quaternion.Inverse(parentWorldRotation) * childWorldRotation;
        Vector3 childLocalEulerAngles = childLocalRotation.eulerAngles;
        Debug.Log(gameObject + " : "+ childLocalEulerAngles);
        //Debug.Log(transform.eulerAngles);
        return childLocalEulerAngles;
    }

    [ContextMenu("Calculate Global")]
    Vector3 CalculateGlobalEuler()
    {
        Vector3 targetDir = transform.position - targetTransform.position;
        //transform.rotation = Quaternion.LookRotation(targetDir, Vector3.forward) * Quaternion.Euler(-90, 0, 0);
        Quaternion rotation = Quaternion.LookRotation(targetDir, Vector3.forward) * Quaternion.Euler(-90, 0, 0);
        Vector3 globalEulerAngles = rotation.eulerAngles;
        Debug.Log(globalEulerAngles);
        return globalEulerAngles;
    }
}

