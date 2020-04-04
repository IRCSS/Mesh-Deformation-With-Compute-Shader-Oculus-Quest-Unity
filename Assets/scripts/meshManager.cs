using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshManager : MonoBehaviour
{
    // =================================================================
    public struct _Vertex
    {
        public float position_x, position_y, position_z;
        public float velocity_x, velocity_y, velocity_z;
        public float uvs_x, uvs_y;

        public _Vertex(Vector3 position, Vector2 uv)
        {
            position_x = position.x;
            position_y = position.y; 
            position_z = position.z;

            velocity_x = 0.0f;
            velocity_y = 0.0f; 
            velocity_z = 0.0f;

            uvs_x = uv.x;
            uvs_y = uv.y;
        }
    }

    // =================================================================

    public ComputeShader m_computeShader;

    // _________________________________

    private _Vertex[]     m_vertexBufferCPU;
    private Vector3[]     m_verticesPosition;       // The original Vertices position of the mesh. Used once on initialize
    private Vector2[]     m_verticesUV;
    


    private ComputeBuffer GPU_VertexBuffer;
    private ComputeBuffer GPU_defaultPositionsBuffer; // Read only Bufffer

    private Mesh          m_mesh;


    private const string kernelName = "CSMain";

    // =================================================================
   void OnDisable()
    {
        GPU_VertexBuffer.Dispose();
        GPU_defaultPositionsBuffer.Dispose();
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

        m_verticesUV = m_mesh.uv;

        m_vertexBufferCPU = new _Vertex[m_verticesPosition.Length];

        for(int i = 0; i< m_vertexBufferCPU.Length; i++)
        {
            _Vertex v = new _Vertex(m_verticesPosition[i], m_verticesUV[i]);
            m_vertexBufferCPU[i] = v;
        }


        Debug.Log(string.Format("Initialized the cpu buffers with {0} vertices, for the compute shader", m_verticesPosition.Length));
    }

    void InitializeGPUBuffers()
    {

        
        int sizeOfVector3 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector3.zero);
        GPU_VertexBuffer = new ComputeBuffer(m_vertexBufferCPU.Length, sizeof(float)*8);
        GPU_VertexBuffer.SetData(m_vertexBufferCPU);

        GPU_defaultPositionsBuffer = new ComputeBuffer(m_verticesPosition.Length, sizeOfVector3);
        GPU_defaultPositionsBuffer.SetData(m_verticesPosition);
        

        int kernel = m_computeShader.FindKernel(kernelName);

        m_computeShader.SetBuffer(kernel, "_VertexBuffer",          GPU_VertexBuffer);
        m_computeShader.SetBuffer(kernel, "_InitialPositionBuffer", GPU_defaultPositionsBuffer);

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
      

        GPU_VertexBuffer.GetData(m_vertexBufferCPU);

        for (int i = 0; i<m_vertexBufferCPU.Length; i++)
        {
            m_verticesPosition[i] = new Vector3(m_vertexBufferCPU[i].position_x, m_vertexBufferCPU[i].position_y, m_vertexBufferCPU[i].position_z);
         
        }
        m_mesh.vertices= m_verticesPosition;
    }

}
