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

    //2019.3버전 실행기준으로 이제 [BurstCompile]어트리뷰트를 달지 않아도 에디터에서 사용체크만 해주면 알아서 해주는걸 확인함.
    //Job Struct안에서 BurstCompile에 포함 시키지 않으려면 해당 함수위에 [BurstDiscard]어트리뷰트를 달아주면 된다.
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
            //부하를 주기위해 말도안되지만 그냥 이렇게 함.
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

        //잡 생성.
        var myMultiJob = new MyMultiJob();
        //잡을 스케쥴에 등록하기
        JobHandle handle = myMultiJob.Schedule(100000000, 64);

        //잡이 끝날때까지 대기.
        handle.Complete();

        sw.Stop();
        Multi.text = sw.ElapsedMilliseconds.ToString() + "ms";
    }
}
