using System;
using System.Reflection;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using Prometheus;

namespace TShockPrometheus {
  [ApiVersion(2, 1)]
  public class TShockPrometheus : TerrariaPlugin {
    ///<summary>
    /// Plugin name
    /// </summary>
    public override string Name { get { return "TShockPrometheus"; } }
    /// <summary>
    /// Plugin version
    /// </summary>
    public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
    /// <summary>
    /// Plugin author(s)
    /// </summary>
    public override string Author { get { return "Swantzter"; } }
    /// <summary>
    /// Plugin description
    /// </summary>
    public override string Description { get { return "Exports TShock data to Prometheus"; } }

    /// <summary>
    /// List of instances of all the registered collectors
    /// </summary>
    private List<Collectors.BaseCollector> collectors = new List<Collectors.BaseCollector>();

    /// <summary>
    /// The Prometheus Metrics server that provides our scrapeable endpoint
    /// </summary>
    private MetricServer server;

    /// <summary>
    /// Initialises Prometheus
    /// </summary>
    /// <param name="game">TShock game</param>
    public TShockPrometheus (Main game) : base(game) {
      Metrics.SuppressDefaultMetrics();
      server = new MetricServer(port: 9763); // TODO: read config

      collectors.Add(new Collectors.ConnectedPlayers(this));
      collectors.Add(new Collectors.DotNetStats(this));
    }

    #region Initialize/Dispose
    /// <summary>
    /// Plugin initialization
    /// </summary>
    public override void Initialize () {
      server.Start();

      for (int idx = 0; idx < collectors.Count; idx++) {
        collectors[idx].Initialize(); // TODO: init based on enabled or not in config
      }
    }

    // TODO: on reload

    /// <summary>
    /// Plugin destruction
    /// </summary>
    protected override void Dispose (bool disposing) {
      if (disposing) {
        server.Stop();
        for (int idx = 0; idx < collectors.Count; idx++) {
          collectors[idx].Dispose();
        }
      }
      base.Dispose(disposing);
    }
    #endregion
  }
}
