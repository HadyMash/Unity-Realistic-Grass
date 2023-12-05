using UnityEngine;

namespace Cube_Test
{
    public class CubeTest : MonoBehaviour
    {
        public Mesh instanceMesh;

        public Material instanceMaterial;

        // public int instanceCount = 1000;
        public int row = 100;
        private readonly uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer _argsBuffer;

        private ComputeBuffer _instanceBuffer;

        private MaterialPropertyBlock _mpb;
        private InstanceData[] instanceData;

        // Start is called before the first frame update
        private void Start()
        {
            _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _mpb = new MaterialPropertyBlock();
            UpdateBuffers();
        }

        // Update is called once per frame
        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial,
                new Bounds(Vector3.zero, Vector3.one * 1000f), _argsBuffer, 0, _mpb);
        }

        private void OnDisable()
        {
            _instanceBuffer?.Release();
            _instanceBuffer = null;

            _argsBuffer?.Release();
            _argsBuffer = null;
        }

        private void UpdateBuffers()
        {
            // positions
            if (_instanceBuffer != null)
                _instanceBuffer.Release();
            _instanceBuffer = new ComputeBuffer(row * row, InstanceData.Size);
            // Vector4[] positions = new Vector4[instanceCount];
            instanceData = new InstanceData[row * row];
            for (var x = 0; x < row; x++)
            for (var z = 0; z < row; z++)
            {
                // positions[i] = new Vector4(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f), 1);
                var position = new Vector3(x, 0, z);
                // var rotation = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f),
                //     Random.Range(0, 360f));
                var rotation = Quaternion.Euler(0f, 90f, 0f);
                var scale = Vector3.one * 0.5f;

                var data = new InstanceData
                {
                    Matrix = Matrix4x4.TRS(position, rotation, scale)
                };
                data.MatrixInverse = data.Matrix.inverse;
                instanceData[x * row + z] = data;
            }


            _instanceBuffer.SetData(instanceData);
            instanceMaterial.SetBuffer("_InstanceData", _instanceBuffer);

            // indirect args
            if (instanceMesh != null)
            {
                _args[0] = instanceMesh.GetIndexCount(0);
                _args[1] = (uint)(row * row);
                _args[2] = instanceMesh.GetIndexStart(0);
                _args[3] = instanceMesh.GetBaseVertex(0);
                _args[4] = 0;
            }
            else
            {
                _args[0] = _args[1] = _args[2] = _args[3] = _args[4] = 0;
            }

            _argsBuffer.SetData(_args);
        }

        private struct InstanceData
        {
            public Matrix4x4 Matrix;
            public Matrix4x4 MatrixInverse;

            public static int Size => sizeof(float) * 16 * 2;
        }
    }
}