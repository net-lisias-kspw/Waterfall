﻿using System.Collections.Generic;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Transform rotation modifier
  /// </summary>
  public class EffectRotationModifier : EffectModifier
  {
    public FloatCurve xCurve = new();
    public FloatCurve yCurve = new();
    public FloatCurve zCurve = new();
    private Vector3 baseRotation;

    public EffectRotationModifier() : base()
    {
      modifierTypeName = "Rotation";
    }

    public EffectRotationModifier(ConfigNode node) : base(node) { }

    public override void Load(ConfigNode node)
    {
      base.Load(node);
      xCurve.Load(node.GetNode("xCurve"));
      yCurve.Load(node.GetNode("yCurve"));
      zCurve.Load(node.GetNode("zCurve"));
    }

    public override ConfigNode Save()
    {
      var node = base.Save();

      node.name = WaterfallConstants.RotationModifierNodeName;
      node.AddNode(Utils.SerializeFloatCurve("xCurve", xCurve));
      node.AddNode(Utils.SerializeFloatCurve("yCurve", yCurve));
      node.AddNode(Utils.SerializeFloatCurve("zCurve", zCurve));
      return node;
    }

    public override void Init(WaterfallEffect parentEffect)
    {
      base.Init(parentEffect);
      baseRotation = xforms[0].localEulerAngles;
    }

    public void Get(float[] input, Vector3[] output) => Get(input, output, xCurve, yCurve, zCurve);

    public override bool IntegratorSuitable(EffectIntegrator integrator) => integrator is EffectRotationIntegrator && integrator.transformName == transformName;

    public override EffectIntegrator CreateIntegrator() => new EffectRotationIntegrator(parentEffect, this);

  }
}
