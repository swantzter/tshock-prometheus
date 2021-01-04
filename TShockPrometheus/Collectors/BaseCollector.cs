using Prometheus;

namespace TShockPrometheus.Collectors {
  public abstract class BaseCollector {

    /// <summary>
    /// Common Prefix for all metric names
    /// </summary>
    private readonly static string PREFIX = "factorio_";

    /// <summary>
    /// If the collection of this metric is enabled or not. set this in
    /// Initialize and Dispose
    /// </summary>
    protected boolean enabled = false;

    protected Registry registry;

    /// <summary>
    /// Constructor
    /// </summary>
    public BaseCollector (Registry promRegistry) {
      this.registry = promRegistry
    }

    #region Initialize/Dispose
    /// <summary>
    /// Hooks into Terraria/Whatever or schedules the collector in some other
    /// way. Such as by timer or prometheus-net's "before collect metrics" thing
    /// and registers the collector into the Prometheus Registry
    /// </summary>
    public abstract void Initialize ();

    /// <summary>
    /// Unhooks Terraria/Whatever and de-registers the collector
    /// from the Prometheus Registry
    /// </summary>
    public abstract void Dispose ();
    #endregion

    /// <summary>
    /// The hook handler this is where you collect data from Terraria or
    /// whatever and records it in the Prometheus Collector
    /// </summary>
    protected abstract void Collect ();

    /// <summary>
    /// Prefixes the metric name with the shared prefix,
    /// should be used by all collectors when creating the Collector
    /// </summary>
    protected string Prefix (string name) => PREFIX + name;
  }
}
