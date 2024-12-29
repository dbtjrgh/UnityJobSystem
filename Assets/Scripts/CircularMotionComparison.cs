using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CircularMotionComparison : MonoBehaviour
{
    public int numberOfSpheres = 100; // 생성할 객체 수
    public GameObject spherePrefab;  // 구체 프리팹
    public bool useJobSystem = true; // Job System 사용 여부를 전환

    private Transform[] sphereTransforms;

    // Job System에서 사용할 데이터
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
        // Sphere 객체를 생성하고 배열 초기화
        sphereTransforms = new Transform[numberOfSpheres];
        positions = new NativeArray<Vector3>(numberOfSpheres, Allocator.Persistent);
        angles = new NativeArray<float>(numberOfSpheres, Allocator.Persistent);

        for (int i = 0; i < numberOfSpheres; i++)
        {
            GameObject sphere = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
            sphereTransforms[i] = sphere.transform;

            // 초기 위치와 각도 설정
            angles[i] = Random.Range(0f, Mathf.PI * 2);
            positions[i] = sphereTransforms[i].position;
        }
    }

    void Update()
    {
        if (useJobSystem)
        {
            UpdateWithJobSystem(); // Job System 사용
        }
        else
        {
            UpdateWithoutJobSystem(); // 메인 스레드 사용
        }
    }

    void UpdateWithJobSystem()
    {
        float radius = 5f;
        float deltaTime = Time.deltaTime;

        // Job 생성 및 스케줄링
        var job = new CircularMotionJobStruct
        {
            deltaTime = deltaTime,
            positions = positions,
            angles = angles,
            radius = radius
        };

        JobHandle handle = job.Schedule(numberOfSpheres, 64);
        handle.Complete();

        // 업데이트된 위치를 객체에 적용
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

            // positions 배열도 업데이트 (비교용)
            positions[i] = newPosition;
        }
    }

    void OnDestroy()
    {
        // NativeArray 메모리 해제
        if (positions.IsCreated) positions.Dispose();
        if (angles.IsCreated) angles.Dispose();
    }
}
