// Adapted from https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/DotNetStats.cs#L27
//

using Prometheus;
using TerrariaApi.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TShockPrometheus.Collectors {
  class DotNetStats : BaseCollector {

    /// <summary>
    /// The current .NET process
    /// </summary>
    static readonly Process process = Process.GetCurrentProcess();

    #region Collectors
    static readonly Counter collectionCountsParent = Metrics.CreateCounter(Prefix("dotnet_collection_count_total"), "GC collection count", new[] { "generation" });
    static readonly List<Counter.Child> collectionCounts = new List<Counter.Child>();
    static Gauge totalMemory = Metrics.CreateGauge(Prefix("dotnet_total_memory_bytes"), "Total known allocated memory");
    static Gauge virtualMemorySize = Metrics.CreateGauge(Prefix("process_virtual_memory_bytes"), "Virtual memory size in bytes.");
    static Gauge workingSet = Metrics.CreateGauge(Prefix("process_working_set_bytes"), "Process working set");
    static Gauge privateMemorySize = Metrics.CreateGauge(Prefix("process_private_memory_bytes"), "Process private memory size");
    static Counter cpuTotal = Metrics.CreateCounter(Prefix("process_cpu_seconds_total"), "Total user and system CPU time spent in seconds.");
    static Gauge openHandles = Metrics.CreateGauge(Prefix("process_open_handles"), "Number of open handles");
    static Gauge startTime = Metrics.CreateGauge(Prefix("process_start_time_seconds"), "Start time of the process since unix epoch in seconds.");
    static Gauge numThreads = Metrics.CreateGauge(Prefix("process_num_threads"), "Total number of threads");
    #endregion

    public DotNetStats (TerrariaPlugin plugin) : base(plugin) {
      for (var gen = 0; gen <= GC.MaxGeneration; gen++)
      {
        collectionCounts.Add(collectionCountsParent.Labels(gen.ToString()));
      }

      var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      startTime.Set((process.StartTime.ToUniversalTime() - epoch).TotalSeconds);
    }

    #region Initialize/Dispose
    /// <summary>
    /// Registers the .NET metrics to the default registry
    /// </summary>
    public override void Initialize () {
      if (enabled) return;

      Metrics.DefaultRegistry.AddBeforeCollectCallback(Collect);

      enabled = true;
    }

    public override void Dispose () {
      if (!enabled) return;

      // TODO: disable. I don't think we can tbh

      // enabled = false;
    }
    #endregion


    #region Hooks
    // The Process class is not thread-safe so let's synchronize the updates to avoid data tearing.
    private readonly object _updateLock = new object();

    /// <summary>
    /// Calles just in time when the prometheus stats are generated for
    /// export
    /// </summary>
    private void Collect () {
      try
      {
        lock (_updateLock)
        {
          process.Refresh();

          for (var gen = 0; gen <= GC.MaxGeneration; gen++)
          {
            var collectionCount = collectionCounts[gen];
            collectionCount.Inc(GC.CollectionCount(gen) - collectionCount.Value);
          }

          totalMemory.Set(GC.GetTotalMemory(false));
          virtualMemorySize.Set(process.VirtualMemorySize64);
          workingSet.Set(process.WorkingSet64);
          privateMemorySize.Set(process.PrivateMemorySize64);
          cpuTotal.Inc(Math.Max(0, process.TotalProcessorTime.TotalSeconds - cpuTotal.Value));
          openHandles.Set(process.HandleCount);
          numThreads.Set(process.Threads.Count);
        }
      }
      catch (Exception)
      {
      }
    }
    #endregion
  }
}
