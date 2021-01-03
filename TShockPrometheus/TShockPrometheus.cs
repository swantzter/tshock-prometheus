using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;

namespace TShockPrometheus {
  [ApiVersion(2, 1)]
  public class TShockPrometheus : TerrariaPlugin {
    public override string Name { get { return "TShockPrometheus"; } }
    public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
    public override string Author { get { return "Swantzter"; } }
    public override string Description { get { return "Exports TShock data to Prometheus"; } }

    /// <summary>
    /// The plugin's constructor
    /// Set your plugin's order (optional) and any other constructor logic here
    /// </summary>
    public TShockPrometheus(Main game) : base(game) {

    }

    /// <summary>
    /// Performs plugin initialization logic.
    /// Add your hooks, config file read/writes, etc here
    /// </summary>
    public override void Initialize() {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Performs plugin cleanup logic
    /// Remove your hooks and perform general cleanup here
    /// </summary>
    protected override void Dispose(bool disposing) {
      if (disposing) {
        //unhook
        //dispose child objects
        //set large objects to null
      }
      base.Dispose(disposing);
    }
  }
}
