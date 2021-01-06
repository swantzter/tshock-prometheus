using System;
using Prometheus;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;

namespace TShockPrometheus.Collectors {
  class TicksPerSecond : BaseCollector {

    /// <summary>
    /// The Prometheus Collector that will hold this metric. This is static
    /// because names are unique, so we couldn't create a copy for each class
    /// instance.
    /// </summary>
    static readonly Gauge collector = Metrics.CreateGauge(Prefix("ticks_per_seconds_average"), "Average Server TPS (ticks per second) since last collection");

    /// <summary>
    /// Max amount of ticks that should happen per second
    /// </summary>
    static readonly int TICKS_PER_SECOND = 60;

    static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private double lastTickTime = 0;
    private float average;
    private float max;
    private float min;


    public TicksPerSecond (TerrariaPlugin plugin) : base(plugin) {
    }

    #region Initialize/Dispose
    /// <summary>
    /// Hook into Terraria, but first check if we should enable or not
    /// </summary>
    public override void Initialize () {
      if (enabled) return;

      ServerApi.Hooks.GameUpdate.Register(plugin, Collect);

      enabled = true;
    }

    public override void Dispose () {
      if (!enabled) return;

      ServerApi.Hooks.GameUpdate.Deregister(plugin, Collect);

      enabled = false;
    }
    #endregion

    #region Hooks
    /// <summary>
    /// Called on each tick
    /// </summary>
    /// <param name="args">event arguments passed by hook</param>
    private void Collect (EventArgs args) {
      var eventTime = (DateTime.UtcNow - epoch).TotalMilliseconds;
      var current = (float)((1 / (eventTime - lastTickTime)) * 1000);
      if (lastTickTime == 0) {
        lastTickTime = eventTime;
        average = current;
        return;
      }

      average = (average + current) / 2;
      if (current > max) max = current;
      if (current < min) min = current;
    }
    #endregion
  }
}
