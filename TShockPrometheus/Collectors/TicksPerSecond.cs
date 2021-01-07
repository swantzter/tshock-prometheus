using System;
using System.Collections.Generic;
using Prometheus;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;

namespace TShockPrometheus.Collectors {
  /// <summary>
  /// Adapted from https://github.com/sladkoff/minecraft-prometheus-exporter/blob/214eaefe0b0aa666a879f126351ea7ce4dbe564c/src/main/java/de/sldk/mc/tps/TpsCollector.java#L1
  /// </summary>
  class TicksPerSecond : BaseCollector {

    /// <summary>
    /// The Prometheus Collector that will hold this metric. This is static
    /// because names are unique, so we couldn't create a copy for each class
    /// instance.
    /// </summary>
    static readonly Gauge collector = Metrics.CreateGauge(Prefix("ticks_per_seconds_average"), "Server TPS (ticks per second) over the last 20 seconds");

    /// <summary>
    /// Max amount of ticks that should happen per second
    /// </summary>
    static readonly ushort TICKS_PER_SECOND = 60;

    /// <summary>
    /// How many ticks should pass in between each round of calculating the TPS
    /// </summary>
    static readonly ushort POLL_INTERVAL = 120; // 2 * 60

    /// <summary>
    /// The amount of
    /// </summary>
    static readonly byte TPS_QUEUE_SIZE = 10;

    /// <summary>
    /// The Unix Epoch
    /// </summary>
    static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The unixtime in milliseconds when we last polled the
    /// <summary>
    private double lastPollTime = (DateTime.UtcNow - epoch).TotalMilliseconds;

    /// <summary>
    /// variable to keep track of how many ticks have passed since we last
    /// made calculations
    /// </summary>
    private ushort tickCounter = 0;

    /// <summary>
    /// We keep a list of TPS_QUEUE_SIZE number of samples of the tps
    /// which we use to calculate the average later
    /// </summary>
    private LinkedList<float> tpsQueue = new LinkedList<float>();

    public TicksPerSecond (TerrariaPlugin plugin) : base(plugin) {
      // See DotNetStats.cs for why we're adding this hook here
      Metrics.DefaultRegistry.AddBeforeCollectCallback(Collect);
    }

    #region Initialize/Dispose
    /// <summary>
    /// Hook into Terraria, but first check if we should enable or not
    /// </summary>
    public override void Initialize () {
      if (enabled) return;

      ServerApi.Hooks.GameUpdate.Register(plugin, OnUpdate);

      enabled = true;
    }

    public override void Dispose () {
      if (!enabled) return;

      ServerApi.Hooks.GameUpdate.Deregister(plugin, OnUpdate);

      enabled = false;
    }
    #endregion

    #region Hooks
    /// <summary>
    /// Called on each tick
    /// </summary>
    /// <param name="args">event arguments passed by hook</param>
    private void OnUpdate (EventArgs args) {
      if (tickCounter < POLL_INTERVAL) {
        tickCounter++;
        return;
      }
      tickCounter = 0;
      var eventTime = (DateTime.UtcNow - epoch).TotalMilliseconds;
      var timeSpent = eventTime - lastPollTime;
      // simple s / (v * t) we reached POLL_INTERVAL number of ticks (s)
      // in timeSpent duration (t), v = s / t
      var tps = (POLL_INTERVAL / (float) timeSpent) * 1000;

      tpsQueue.AddLast(tps > TICKS_PER_SECOND ? TICKS_PER_SECOND : tps);

      if (tpsQueue.Count > TPS_QUEUE_SIZE) {
        // ensure size of our queue of tps metrics
        tpsQueue.RemoveFirst();
      }

      lastPollTime = eventTime;
    }

    private void Collect () {
      if (!enabled) return;
      lock (tpsQueue) {
        float sum = 0;
        foreach (float tps in tpsQueue) {
          sum += tps;
        }
        collector.Set(sum / tpsQueue.Count);
      }
    }
    #endregion
  }
}
