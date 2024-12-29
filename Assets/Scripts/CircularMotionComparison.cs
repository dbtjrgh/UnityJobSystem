using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CircularMotionComparison : MonoBehaviour
{
    public int numberOfSpheres = 100; // ������ ��ü ��
    public GameObject spherePrefab;  // ��ü ������
    public bool useJobSystem = true; // Job System ��� ���θ� ��ȯ

    private Transform[] sphereTransforms;

    // Job System���� ����� ������
    private NativeArray<Vector3> positions;
    private NativeArray<float> angles;

    [BurstCompile]
    struct CircularMotionJobStruct : IJobParallelFor
    {
        public float deltaTime;
        public NativeArray<Vector3> positions;
        public NativeArray<float> angles;
        public float radius;

        public void Execute(int index)
        {
            angles[index] += deltaTime;
            float x = Mathf.Cos(angles[index]) * radius;
            float z = Mathf.Sin(angles[index]) * radius;
            positions[index] = new Vector3(x, 0, z);
        }
    }

    void Start()
    {
        // Sphere ��ü�� �����ϰ� �迭 �ʱ�ȭ
        sphereTransforms = new Transform[numberOfSpheres];
        positions = new NativeArray<Vector3>(numberOfSpheres, Allocator.Persistent);
        angles = new NativeArray<float>(numberOfSpheres, Allocator.Persistent);

        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
            sphereTransforms[i] = sphere.transform;

            // �ʱ� ��ġ�� ���� ����
            angles[i] = Random.Range(0f, Mathf.PI * 2);
            positions[i] = sphereTransforms[i].position;
        }
    }

    void Update()
    {
        if (useJobSystem)
        {
            UpdateWithJobSystem(); // Job System ���
        }
        else
        {
            UpdateWithoutJobSystem(); // ���� ������ ���
        }
    }

    void UpdateWithJobSystem()
    {
        float radius = 5f;
        float deltaTime = Time.deltaTime;

        // Job ���� �� �����ٸ�
        var job = new CircularMotionJobStruct
        {
            deltaTime = deltaTime,
            positions = positions,
            angles = angles,
            radius = radius
        };

        JobHandle handle = job.Schedule(numberOfSpheres, 64);
        handle.Complete();

        // ������Ʈ�� ��ġ�� ��ü�� ����
        for (int i = 0; i < sphereTransforms.Length; i++)
        {
            sphereTransforms[i].position = positions[i];
        }
    }

    void UpdateWithoutJobSystem()
    {
        float radius = 5f;
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < numberOfSpheres; i++)
        {
            angles[i] += deltaTime;
            float x = Mathf.Cos(angles[i]) * radius;
            float z = Mathf.Sin(angles[i]) * radius;
            Vector3 newPosition = new Vector3(x, 0, z);
            sphereTransforms[i].position = newPosition;

            // positions �迭�� ������Ʈ (�񱳿�)
            positions[i] = newPosition;
        }
    }

    void OnDestroy()
    {
        // NativeArray �޸� ����
        if (positions.IsCreated) positions.Dispose();
        if (angles.IsCreated) angles.Dispose();
    }
}
