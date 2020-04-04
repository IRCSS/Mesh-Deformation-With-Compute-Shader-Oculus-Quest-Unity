using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshManager : MonoBehaviour
{
    // =================================================================
    //public struct _Vertex
    //{
    //    public float[3] position;
    //    public float[3] velocity;
    //    public float[2] uvs;
    //}

    // =================================================================

    public ComputeShader m_computeShader;

    // _________________________________

    private Vector3[]     m_verticesPosition;       // The original Vertices position of the mesh. Used once on initialize
    private Vector3[]     m_verticesModified;       // Updated every frame to modified vertices positions 

    private ComputeBuffer m_PositionBuffer;         // Modified in compute based on velocity buffer. Pulled every frame to update mesh data.
    private ComputeBuffer m_VelocityBuffer;         // Modified in compute shader based on user input
    private ComputeBuffer m_defaultPositionsBuffer; // Read only Bufffer

    private Mesh          m_mesh;


    private const string kernelName = "CSMain";

    // =================================================================
    ~meshManager()
    {
        m_PositionBuffer.Dispose();
        m_VelocityBuffer.Dispose();
        m_defaultPositionsBuffer.Dispose();
    }


    void Start()
    {
        InitializeMesh();
        InitializeCPUBuffers();
        InitializeGPUBuffers();
        InitializeShaderParameters();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateRuntimeShaderParameter();
        RunShader();
        PullResults();
    }


    // =================================================================
    void InitializeMesh()
    {
        MeshFilter mf = this.transform.GetChild(0).GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.LogError(string.Format("Gameobject {0}, attmpted to access non exisitng MeshFilter", this.gameObject.name));
            return;
        }

              m_mesh = mf.mesh;
        if (m_mesh == null)
        {
            Debug.LogError(string.Format("No Mehs assigned to the Meshfilter component of GameObject {0}", this.gameObject.name));
            return;
        }

        mf.sharedMesh = m_mesh;

        Debug.Log(string.Format("Initialized the mesh: {0}, for the compute shader", m_mesh));
    }

    void InitializeCPUBuffers()
    {
        m_verticesPosition = m_mesh.vertices;
        m_verticesModified = new Vector3[m_verticesPosition.Length];

        Debug.Log(string.Format("Initialized the cpu buffers with {0} vertices, for the compute shader", m_verticesPosition.Length));
    }

    void InitializeGPUBuffers()
    {

        
        int sizeOfVector3 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector3.zero);
         m_PositionBuffer = new ComputeBuffer(m_verticesPosition.Length, sizeOfVector3);
         m_PositionBuffer.SetData(m_verticesPosition);

        m_defaultPositionsBuffer = new ComputeBuffer(m_verticesPosition.Length, sizeOfVector3);
        m_defaultPositionsBuffer.SetData(m_verticesPosition);

         m_VelocityBuffer = new ComputeBuffer(m_verticesPosition.Length, sizeOfVector3);

        int kernel = m_computeShader.FindKernel(kernelName);

        m_computeShader.SetBuffer(kernel, "_VelocityBuffer",        m_VelocityBuffer);
        m_computeShader.SetBuffer(kernel, "_PositionBuffer",        m_PositionBuffer);
        m_computeShader.SetBuffer(kernel, "_InitialPositionBuffer", m_defaultPositionsBuffer);

        Debug.Log(string.Format("Initialized the GPU buffers with {0} vertices, for the compute shader", m_verticesPosition.Length));

    }

   void InitializeShaderParameters()
    {
        Debug.Log(string.Format("Initialized Shader Parameters"));
    }

    void UpdateRuntimeShaderParameter()
    {
        m_computeShader.SetFloat("_Time", Time.time);
    }

    void RunShader()
    {
        int kernel = m_computeShader.FindKernel(kernelName);
        m_computeShader.Dispatch(kernel,60000, 1, 1);

        if (Time.time % 5 == 0) Debug.Log("Running Compute Shader");
    }

    void PullResults()
    {
        
        m_PositionBuffer.GetData(m_verticesModified);
        m_mesh.vertices = m_verticesModified;
        
    }

}
