using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class SpawnParallel : MonoBehaviour
{
    public GameObject cubePrefab; // 박스 프리팹
    Transform[] AllCubeTransform; // 모든 네모의 트랜스폼 배열
    public int numCube = 10000; // 네모의 개수 설정

    struct MoveJob : IJobParallelForTransform
    {
        public void Execute(int index, TransformAccess transform)
        {
            // 이동 작업을 수행하는 잡(Job) 구조체

            // 네모를 위으로 이동시킵니다.
            transform.position += 0.1f * (transform.rotation * new Vector3(0, 1, 0));

            // 네모의 일정 높이(여기서는 50)를 넘어가면 땅으로 되돌립니다.
            if (transform.position.y > 50)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
    }

    MoveJob moveJob; // 이동 작업을 위한 잡(Job)
    JobHandle moveHandle; // 작업 핸들
    TransformAccessArray transforms; // 네모들의 트랜스폼에 대한 배열

    // Start is called before the first frame update
    void Start()
    {
        // 네모들의 트랜스폼 배열 초기화
        AllCubeTransform = new Transform[numCube];

        // 지정된 개수만큼 네모를 무작위 위치에 생성하고 트랜스폼을 배열에 저장
        for (int i = 0; i < numCube; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), Random.Range(-50, 50));
            GameObject obj = Instantiate(cubePrefab, pos, Quaternion.identity);
            AllCubeTransform[i] = obj.transform;
            Debug.Log(i + "번째 Cube 생성");
        }

        // 네모들의 트랜스폼에 대한 접근 배열 생성
        transforms = new TransformAccessArray(AllCubeTransform);
    }

    private void Update()
    {
        // 이동 작업을 수행할 잡(Job) 생성
        moveJob = new MoveJob { };

        // 잡(Job)을 스케줄링하여 네모들의 이동 수행
        moveHandle = moveJob.Schedule(transforms);
    }

    private void LateUpdate()
    {
        // 이동 작업이 완료될 때까지 대기
        moveHandle.Complete();
    }

    private void OnDestroy()
    {
        // 사용한 자원 정리
        transforms.Dispose();
    }
}
