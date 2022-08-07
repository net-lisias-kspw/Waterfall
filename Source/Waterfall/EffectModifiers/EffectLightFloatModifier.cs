﻿using System.Collections.Generic;
using UnityEngine;

namespace Waterfall
{
  /// <summary>
  ///   Material color modifier
  /// </summary>
  public class EffectLightFloatModifier : EffectModifier
  {
    [Persistent] public string floatName = "";
    public FloatCurve curve = new();
    private Light[] l;
    public override bool ValidForIntegrator => !string.IsNullOrEmpty(floatName);

    public EffectLightFloatModifier() : base()
    {
      modifierTypeName = "Light Float";
    }

    public EffectLightFloatModifier(ConfigNode node) : base(node) { }

    public override void Load(ConfigNode node)
    {
      base.Load(node);
      curve.Load(node.GetNode("floatCurve"));
    }

    public override ConfigNode Save()
    {
      var node = base.Save();

      node.name = WaterfallConstants.LightFloatModifierNodeName;
      node.AddNode(Utils.SerializeFloatCurve("floatCurve", curve));
      return node;
    }

    public override void Init(WaterfallEffect parentEffect)
    {
      base.Init(parentEffect);
      l = new Light[xforms.Count];
      for (int i = 0; i < xforms.Count; i++)
      {
        l[i] = xforms[i].GetComponent<Light>();
      }
    }

    public void Get(float[] input, float[] output)
    {
      if (input.Length > 1)
      {
        for (int i = 0; i < l.Length; i++)
          output[i] = curve.Evaluate(input[i]) + randomValue;
      }
      else if (input.Length == 1)
      {
        float data = curve.Evaluate(input[0]) + randomValue;
        for (int i = 0; i < l.Length; i++)
          output[i] = data;
      }
    }

    public Light GetLight() => l[0];

    public void ApplyFloatName(string newFloatName)
    {
      floatName = newFloatName;
      parentEffect.ModifierParameterChange(this);
    }

    public override bool IntegratorSuitable(EffectIntegrator integrator) => integrator is EffectLightFloatIntegrator i && i.floatName == floatName && integrator.transformName == transformName;

    public override EffectIntegrator CreateIntegrator() => new EffectLightFloatIntegrator(parentEffect, this);
  }
}
