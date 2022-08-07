﻿using System;
using UnityEngine;

namespace Waterfall.UI.EffectControllersUI
{
  public class RCSControllerUIOptions : DefaultEffectControllerUIOptions<RCSController>
  {
    private readonly string[]    throttleStrings;

    private float rampRateUp   = 100f;
    private float rampRateDown = 100f;

    public RCSControllerUIOptions()
    {
      throttleStrings   = new[] { rampRateUp.ToString(), rampRateDown.ToString() };
    }

    public override void DrawOptions()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Ramp Rate Up", UIResources.GetStyle("data_header"), GUILayout.MaxWidth(160f));
      throttleStrings[0] = GUILayout.TextArea(throttleStrings[0], GUILayout.MaxWidth(60f));
      float floatParsed;
      if (Single.TryParse(throttleStrings[0], out floatParsed))
      {
        rampRateUp = floatParsed;
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Ramp Rate Down", UIResources.GetStyle("data_header"), GUILayout.MaxWidth(160f));
      throttleStrings[1] = GUILayout.TextArea(throttleStrings[1], GUILayout.MaxWidth(60f));
      if (Single.TryParse(throttleStrings[1], out floatParsed))
      {
        rampRateDown = floatParsed;
      }

      GUILayout.EndHorizontal();
    }

    protected override void LoadOptions(RCSController controller)
    {
      throttleStrings[0] = controller.responseRateUp.ToString();
      throttleStrings[1] = controller.responseRateDown.ToString();
    }

    protected override RCSController CreateControllerInternal() =>
      new()
      {
        responseRateUp   = rampRateUp,
        responseRateDown = rampRateDown
      };
  }
}