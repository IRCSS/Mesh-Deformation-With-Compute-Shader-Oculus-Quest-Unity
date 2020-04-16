using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManagement : MonoBehaviour
{

    public Transform RightHandCollider, LeftHandCollider;


    public OVRHand leftHand;
    public OVRHand rightHand;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (leftHand.IsTracked) LeftHandCollider.transform.localPosition = new Vector3(0.1074f, 0.0f, -0.001100004f);
        else LeftHandCollider.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        if (rightHand.IsTracked) RightHandCollider.transform.localPosition = new Vector3(-0.1074f, 0.0f, -0.001100004f);
        else RightHandCollider.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

    }
}
