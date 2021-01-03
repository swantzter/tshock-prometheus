using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Prometheus;

namespace TShockPrometheus {
  [ApiVersion(2, 1)]
  public class TShockPrometheus : TerrariaPlugin {
    public override string Name { get { return "TShockPrometheus"; } }
    public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
    public override string Author { get { return "Swantzter"; } }
    public override string Description { get { return "Exports TShock data to Prometheus"; } }

    private static readonly Gauge ConnectedPlayersGauge = Metrics.CreateGauge("tshock_connected_player_count", "connected players");
    private static MetricServer server;

    /// <summary>
    /// The plugin's constructor
    /// Set your plugin's order (optional) and any other constructor logic here
    /// </summary>
    public TShockPrometheus(Main game) : base(game) {
      server = new MetricServer(hostname: "localhost", port: 9763);
    }

    #region Initialize/Dispose
    public override void Initialize() {
      server.Start();

      ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
      ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        server.Stop();

        ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
        ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
      }
      base.Dispose(disposing);
    }
    #endregion

    #region Hooks
    private void OnLeave(LeaveEventArgs args) {
      if (TShock.Players[args.Who] == null) return;
      ConnectedPlayersGauge.Dec(1);
    }

    private void OnJoin(JoinEventArgs args) {
      ConnectedPlayersGauge.Inc(1);
    }
    #endregion
  }
}
