﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Waterfall
{
  [DisplayName("Gimbal")]
  public class GimbalController : WaterfallController
  {
    public  float        atmosphereDepth = 1;
    public  string       axis            = "x";
    private ModuleGimbal gimbalController;

    public GimbalController() { }

    public GimbalController(ConfigNode node)
    {
      node.TryGetValue(nameof(axis), ref axis);
      node.TryGetValue(nameof(name), ref name);
    }

    public override ConfigNode Save()
    {
      var c = base.Save();
      c.AddValue(nameof(axis), axis);
      return c;
    }

    public override void Initialize(ModuleWaterfallFX host)
    {
      base.Initialize(host);

      gimbalController = host.GetComponents<ModuleGimbal>().ToList().First();

      if (gimbalController == null)
        Utils.LogError("[GimbalController] Could not find gimbal controller on Initialize");
    }

    public override List<float> Get()
    {
      if (overridden)
        return new() { overrideValue };

      if (gimbalController == null)
      {
        Utils.LogWarning("[GimbalController] Gimbal controller not assigned");
        return new() { 0f };
      }

      if (axis == "x")
        return new() { gimbalController.actuationLocal.x / gimbalController.gimbalRangeXP };
      if (axis == "y")
        return new() { gimbalController.actuationLocal.y / gimbalController.gimbalRangeYP };
      if (axis == "z")
        return new() { gimbalController.actuationLocal.z };

      return new() { 0f };
    }
  }
}