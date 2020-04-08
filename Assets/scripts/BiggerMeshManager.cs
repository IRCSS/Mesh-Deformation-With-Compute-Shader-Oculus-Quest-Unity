using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiggerMeshManager : MonoBehaviour
{
    public meshManager m_smallMeshManager;

    private ComputeBuffer m_vertrexBuffer;
    private Material m_smallerMeshMaterial;
    // Start is called before the first frame update
    void Start()
    {
        m_vertrexBuffer = m_smallMeshManager.GetVertexBuffer();

        if (m_vertrexBuffer == null) Debug.LogErrorFormat("Attempted to get the vertex buffer from the mesh Manager, however no buffer was initalized");

        m_smallerMeshMaterial = this.transform.GetComponentInChildren<Renderer>().material;
        m_smallerMeshMaterial.SetBuffer("_VertexBuffer", m_vertrexBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
