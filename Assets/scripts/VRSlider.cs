using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSlider : MonoBehaviour
{

    public enum VariableType { elasticity};

    public meshManager r_meshManager;

    public VariableType ValueName;
    public float        min;
    public float        max;
                        

    Transform pointer;
    Transform begin;
    Transform end;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < this.transform.childCount;  i++)
        {
            Transform t= this.transform.GetChild(i);
            if (t.gameObject.name == "Pointer") pointer = t;
            if (t.gameObject.name == "Begin")   begin = t;
            if (t.gameObject.name == "End")     end= t;
        }

    }

    void SetValue()
    {
        Vector3 beginToEnd     = end.position     - begin.position;
        Vector3 beginToPointer = pointer.position - begin.position;



        float value = beginToPointer.magnitude / beginToEnd.magnitude;
        value = Mathf.Clamp01(value);
        value = min + (max - min) * value;
        
        switch (ValueName)
        {
            case VariableType.elasticity: r_meshManager.SetElacticty(value); break;
        }
            

    }

    // Update is called once per frame
    void Update()
    {
        SetValue();
    }
}
