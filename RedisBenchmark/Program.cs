using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Ingestion.Api.Dto;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<RedisBenchmark>();
    }
}

[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
public class RedisBenchmark
{
    private SignalDataDto[] signals;
    private ConnectionMultiplexer redis;
    private IDatabase db;
    private ISubscriber pubsub;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Initialize the ConnectionMultiplexer once before running the benchmarks
        redis = ConnectionMultiplexer.Connect("");
        db = redis.GetDatabase();
        pubsub = redis.GetSubscriber();
        signals = new[]
        {
            new SignalDataDto("Device1", "Object1", 1621123456, "Value1"),
            new SignalDataDto("Device2", "Object2", 1621123457, "Value2"),
            new SignalDataDto("Device3", "Object3", 1621123458, "Value3"),
            new SignalDataDto("Device4", "Object4", 1621123459, "Value4"),
            new SignalDataDto("Device5", "Object5", 1621123460, "Value5"),
            new SignalDataDto("Device6", "Object6", 1621123461, "Value6"),
            new SignalDataDto("Device7", "Object7", 1621123462, "Value7"),
            new SignalDataDto("Device8", "Object8", 1621123463, "Value8"),
            new SignalDataDto("Device9", "Object9", 1621123464, "Value9"),
            new SignalDataDto("Device10", "Object10", 1621123465, "Value10")
        };
        _ = signals.Select(x => x.HashCode);

    }

    /*
    |                                 Method |      Mean |    Error |   StdDev | Ratio |
    |--------------------------------------- |----------:|---------:|---------:|------:|
    |         InsertSignalsOnDatabaseBatched |  21.41 ms | 0.231 ms | 0.205 ms |  1.00 |
    |                                        |           |          |          |       |
    | InsertSignalsOnDatabaseNotBatchedAsync | 211.83 ms | 2.597 ms | 2.429 ms |  1.00 |
    |                                        |           |          |          |       |
    |                    SendSignalsOnPubSub |  20.95 ms | 0.248 ms | 0.232 ms |  1.00 | 
     */

    [Benchmark]
    public void InsertSignalsOnDatabaseBatched()
    {
        var batch = db.CreateBatch();
        var tasks = GetInsertions().ToArray();
        batch.Execute();

        Task.WaitAll(tasks);

        IEnumerable<Task<bool>> GetInsertions()
        {
            foreach (var signal in signals)
                yield return batch.SetAddAsync(signal.HashCode.ToString(), JsonConvert.SerializeObject(signal));
        }
    }

    [Benchmark]
    public async Task InsertSignalsOnDatabaseNotBatchedAsync()
    {
        foreach (var signal in signals)
            await db.SetAddAsync(signal.HashCode.ToString(), JsonConvert.SerializeObject(signal));
    }

    [Benchmark]
    public void SendSignalsOnPubSub()
    {
        pubsub.WaitAll(GetPublications().ToArray());

        IEnumerable<Task> GetPublications()
        {
            foreach (var signal in signals)
                yield return pubsub.PublishAsync("benchmarkChannel", JsonConvert.SerializeObject(signal));
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        redis.Dispose();
    }
}
