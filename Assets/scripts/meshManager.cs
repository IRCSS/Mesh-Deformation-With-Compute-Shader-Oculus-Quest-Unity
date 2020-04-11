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

    public ComputeShader  m_computeShader;
    public Transform      m_RightHandCollider;
    public Transform      m_leftHandCollider;

    [Header("Balancing Parameters")]
    public float m_pushforce              = 1.0f;
    public float m_drag                   = 0.01f;
    public float m_elasticity             = 0.01f;
    public float m_colliderBeginDistance  = 2.0f;
    public float m_colliderEndDistance    = 4.0f;


    // _________________________________

    private _Vertex[]     m_vertexBufferCPU;
    private Vector3[]     m_verticesPosition;       // The original Vertices position of the mesh. Used once on initialize
    private Vector2[]     m_verticesUV;
    private int[]         m_indexBuffer;


    private ComputeBuffer GPU_VertexBuffer;
    private ComputeBuffer GPU_defaultPositionsBuffer; // Read only Bufffer



    private Mesh           m_mesh;
    private Material[]     m_mats;

    private const string kernelName = "CSMain";

    private Vector3 m_rightControllerLastFramePostion;
    private Vector3 m_LeftControllerLastFramePostion;


    // =================================================================
    void OnDisable()
    {
        GPU_VertexBuffer.Dispose();
        GPU_defaultPositionsBuffer.Dispose();
    }


    void Start()
    {
        m_rightControllerLastFramePostion = m_RightHandCollider.transform.position;
        m_LeftControllerLastFramePostion  = m_leftHandCollider.transform.position;
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
    }


    public ComputeBuffer GetVertexBuffer()
    {
        return GPU_VertexBuffer;
    }

    public void SetElacticty(float toSet) { m_elasticity = toSet; }

    // magintude of a vector in world space is sqrt(x^2 +y^2 + z^2). The scaling matrix is diagonal and multiplies the xyz component so, 
    // the magnitude of any given vector would be sqrt((x*scale.x)^2 + (x*scale.y)^2+ (x*scale.z)^2) in local space
    // in this case, I will only allow uniform scaling, so scale.x= scale.y = scale.z = scalef
    // this means we can factor the scale out and we will have worldMag / scalef = meshMagnitude
    private float ScaleFromWorldtoMeshSpace(float scale)
    {
        float scalef = this.transform.localScale.x * this.transform.GetChild(0).transform.localScale.x;
        return scale / scalef;
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


        m_indexBuffer = m_mesh.triangles;


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

        m_computeShader.SetFloat("_distanceBegin", ScaleFromWorldtoMeshSpace(m_colliderBeginDistance));
        m_computeShader.SetFloat("_distnaceEnd"  , ScaleFromWorldtoMeshSpace(m_colliderEndDistance  ));
        m_computeShader.SetFloat("_pushforce"    , m_pushforce                                       );
        m_computeShader.SetFloat("_elacticity"   , m_elasticity                                      );
        m_computeShader.SetFloat("_drag"         , m_drag                                            );

        Debug.Log(string.Format("Initialized the GPU buffers with {0} vertices, for the compute shader", m_verticesPosition.Length));

    }

   void InitializeShaderParameters()
    {

        

        // Mesh shader parameters
        Renderer meshRenderer = this.transform.GetChild(0).GetComponent<Renderer>();
        if(meshRenderer == null)
        {
            Debug.LogError(string.Format("Attempted to acces non exisiting mesh Renderer, on game Object {0}", this.gameObject.name));
            return;
        }

        m_mats = meshRenderer.materials;
        
        foreach(Material m in m_mats)
        {
            m.SetBuffer("_VertexBuffer", GPU_VertexBuffer);
        }
        

        Debug.Log(string.Format("Initialized Shader Parameters. {0} materials were found", m_mats.Length));
    }

    void UpdateRuntimeShaderParameter()
    {
        // ----------------------------------------------------------------------------------------------------------------------------
        m_computeShader.SetFloat("_distanceBegin", ScaleFromWorldtoMeshSpace(m_colliderBeginDistance));
        m_computeShader.SetFloat("_distnaceEnd"  , ScaleFromWorldtoMeshSpace(m_colliderEndDistance  ));
        m_computeShader.SetFloat("_pushforce"    , m_pushforce                                       );
        m_computeShader.SetFloat("_elacticity"   , m_elasticity                                      );
        m_computeShader.SetFloat("_drag"         , m_drag                                            );


        // ----------------------------------------------------------------------------------------------------------------------------
        // Right hand stuff
        Vector3 posInObjectLocal = this.transform.GetChild(0).transform.worldToLocalMatrix
            * new Vector4(m_RightHandCollider.position.x, m_RightHandCollider.position.y, m_RightHandCollider.position.z, 1.0f);
        
        m_computeShader.SetVector("_RHandPosition", posInObjectLocal);

        Vector3 rightHandVel = m_RightHandCollider.transform.position - m_rightControllerLastFramePostion;
        m_rightControllerLastFramePostion = m_RightHandCollider.transform.position;

        rightHandVel = this.transform.GetChild(0).transform.worldToLocalMatrix * new Vector4(rightHandVel.x, rightHandVel.y, rightHandVel.z, 0.0f);
        m_computeShader.SetVector("_RHandVelocity", rightHandVel);

        // ----------------------------------------------------------------------------------------------------------------------------
        // left hand stuff
        posInObjectLocal = this.transform.GetChild(0).transform.worldToLocalMatrix 
            * new Vector4(m_leftHandCollider.position.x, m_leftHandCollider.position.y, m_leftHandCollider.position.z, 1.0f);
        m_computeShader.SetVector("_LHandPosition", posInObjectLocal);
        Vector3 leftHandVel = m_leftHandCollider.transform.position - m_LeftControllerLastFramePostion;
        m_LeftControllerLastFramePostion = m_leftHandCollider.transform.position;

        leftHandVel = this.transform.GetChild(0).transform.worldToLocalMatrix * new Vector4(leftHandVel.x, leftHandVel.y, leftHandVel.z, 0.0f);
        m_computeShader.SetVector("_LHandVelocity", leftHandVel);

        // ----------------------------------------------------------------------------------------------------------------------------
        m_computeShader.SetFloat("_Time", Time.time);
    }

    void RunShader()
    {
        int kernel = m_computeShader.FindKernel(kernelName);
        m_computeShader.Dispatch(kernel,60000, 1, 1);
        
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
