using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Unity.Burst;

public class ThredTimeTest : MonoBehaviour
{
    [SerializeField] 
    Text Single;
    [SerializeField] 
    Text Multi;

    //2019.3���� ����������� ���� [BurstCompile]��Ʈ����Ʈ�� ���� �ʾƵ� �����Ϳ��� ���üũ�� ���ָ� �˾Ƽ� ���ִ°� Ȯ����.
    //Job Struct�ȿ��� BurstCompile�� ���� ��Ű�� �������� �ش� �Լ����� [BurstDiscard]��Ʈ����Ʈ�� �޾��ָ� �ȴ�.
    struct Myjob : IJob
    {
        public void Execute()
        {
            var Temp = 0;

            for (int i = 0; i < 100000000; i++)
            {
                Temp += i;
            }
        }
    }

    struct MyMultiJob : IJobParallelFor
    {
        public void Execute(int index)
        {
            //���ϸ� �ֱ����� �����ȵ����� �׳� �̷��� ��.
            var Temp = index;
        }
    }
    public void _RunSingle()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var Temp = 0;

        for (int i = 0; i < 100000000; i++)
        {
            Temp += i;
        }

        sw.Stop();
        Single.text = sw.ElapsedMilliseconds.ToString() + "ms";
    }

    public void _RunMulti()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        //�� ����.
        var myMultiJob = new MyMultiJob();
        //���� �����쿡 ����ϱ�
        JobHandle handle = myMultiJob.Schedule(100000000, 64);

        //���� ���������� ���.
        handle.Complete();

        sw.Stop();
        Multi.text = sw.ElapsedMilliseconds.ToString() + "ms";
    }
}
