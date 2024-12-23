using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class SpawnParallel : MonoBehaviour
{
    public GameObject cubePrefab; // �ڽ� ������
    Transform[] AllCubeTransform; // ��� �׸��� Ʈ������ �迭
    public int numCube = 10000; // �׸��� ���� ����

    struct MoveJob : IJobParallelForTransform
    {
        public void Execute(int index, TransformAccess transform)
        {
            // �̵� �۾��� �����ϴ� ��(Job) ����ü

            // �׸� ������ �̵���ŵ�ϴ�.
            transform.position += 0.1f * (transform.rotation * new Vector3(0, 1, 0));

            // �׸��� ���� ����(���⼭�� 50)�� �Ѿ�� ������ �ǵ����ϴ�.
            if (transform.position.y > 50)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
    }

    MoveJob moveJob; // �̵� �۾��� ���� ��(Job)
    JobHandle moveHandle; // �۾� �ڵ�
    TransformAccessArray transforms; // �׸���� Ʈ�������� ���� �迭

    // Start is called before the first frame update
    void Start()
    {
        // �׸���� Ʈ������ �迭 �ʱ�ȭ
        AllCubeTransform = new Transform[numCube];

        // ������ ������ŭ �׸� ������ ��ġ�� �����ϰ� Ʈ�������� �迭�� ����
        for (int i = 0; i < numCube; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), Random.Range(-50, 50));
            GameObject obj = Instantiate(cubePrefab, pos, Quaternion.identity);
            AllCubeTransform[i] = obj.transform;
            Debug.Log(i + "��° Cube ����");
        }

        // �׸���� Ʈ�������� ���� ���� �迭 ����
        transforms = new TransformAccessArray(AllCubeTransform);
    }

    private void Update()
    {
        // �̵� �۾��� ������ ��(Job) ����
        moveJob = new MoveJob { };

        // ��(Job)�� �����ٸ��Ͽ� �׸���� �̵� ����
        moveHandle = moveJob.Schedule(transforms);
    }

    private void LateUpdate()
    {
        // �̵� �۾��� �Ϸ�� ������ ���
        moveHandle.Complete();
    }

    private void OnDestroy()
    {
        // ����� �ڿ� ����
        transforms.Dispose();
    }
}
