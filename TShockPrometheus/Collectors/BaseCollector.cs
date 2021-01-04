using Prometheus;
using TerrariaApi.Server;

namespace TShockPrometheus.Collectors {
  public abstract class BaseCollector {

    /// <summary>
    /// Common Prefix for all metric names
    /// </summary>
    private readonly static string PREFIX = "tshock_";

    public static string name;

    /// <summary>
    /// Whether or not the Collector is enabled
    /// </summary>
    public bool enabled = false;

    /// <summary>
    /// The terraria plugin this collector is part of
    /// </summary>
    protected TerrariaPlugin plugin;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="plugin">The Terraria Plugin</param>
    public BaseCollector (TerrariaPlugin plugin) {
      this.plugin = plugin;
    }

    #region Initialize/Dispose
    /// <summary>
    /// This function should hook into the appropriate Terraria/TShock/Whatever
    /// Hooks, or enqueue the collection of metrics somehow.
    ///
    /// It should not run if enabled is true, and it should set enabled to true
    /// once it is done.
    /// </summary>
    public abstract void Initialize ();

    /// <summary>
    /// This function should unhook/undo whatever Initialize did so that the
    /// metric is no longer collected.
    ///
    /// It should not run if enabled is false, and it should set enabled to
    /// false once it is done.
    /// </summary>
    public abstract void Dispose ();
    #endregion

    /// <summary>
    /// Prefixes the metric name with the shared prefix,
    /// should be used by all collectors when creating the Collector
    /// </summary>
    protected static string Prefix (string name) => PREFIX + name;
  }
}
