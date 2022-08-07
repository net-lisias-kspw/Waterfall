﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Waterfall
{
  public enum EffectModifierMode
  {
    REPLACE,
    ADD,
    SUBTRACT,
    MULTIPLY
  }

  /// <summary>
  ///   Base effect modifer class
  /// </summary>
  public abstract class EffectModifier
  {
    // This is the game name of the effect
    public string fxName = "";

    // This is the name of the controller that should be associated with the module
    [Persistent] public string controllerName = "";
    [Persistent] public string transformName = "";
    public string modifierTypeName = "";

    [Persistent] public bool useRandomness;
    [Persistent] public string randomnessController = nameof(RandomnessController);
    [Persistent] public float randomnessScale = 1f;

    public WaterfallEffect parentEffect;

    // The combination mode of this effect with the base
    public EffectModifierMode effectMode = EffectModifierMode.REPLACE;

    // The Transform that holds the thing the effect should modify
    protected List<Transform> xforms;
    protected float randomValue;

    public EffectIntegrator integrator;
    public WaterfallController Controller { get; private set; }
    private WaterfallController randomController;
    public virtual bool ValidForIntegrator => true;

    public EffectModifier()
    {
      xforms = new();
    }

    public EffectModifier(ConfigNode node) : this()
    {
      Load(node);
    }

    public virtual void Load(ConfigNode node)
    {
      ConfigNode.LoadObjectFromConfig(this, node);
      node.TryGetValue("name", ref fxName);
      node.TryGetEnum("combinationType", ref effectMode, EffectModifierMode.REPLACE);
      Utils.Log($"[EffectModifier]: Loading modifier {fxName}", LogType.Modifiers);
    }

    /// <summary>
    ///   Initialize the effect
    /// </summary>
    public virtual void Init(WaterfallEffect effect)
    {
      parentEffect = effect;
      Controller = parentEffect.parentModule.AllControllersDict.TryGetValue(controllerName, out var controller) ? controller : null;
      if (Controller == null)
      {
        Utils.LogError($"[EffectModifier]: Controller {controllerName} not found for modifier {fxName} in effect {effect.name} in module {effect.parentModule.moduleID}");
      }
      else
      {
        Controller.referencingModifierCount++;
      }

      randomController = parentEffect.parentModule.AllControllersDict.TryGetValue(randomnessController, out controller) ? controller : null;

      if (randomController == null && useRandomness)
      {
        Utils.LogError($"[EffectModifier]: Randomness controller {randomnessController} not found for modifier {fxName} in effect {effect.name} in module {effect.parentModule.moduleID}");
      }
      else if (randomController != null)
      {
        randomController.referencingModifierCount++;
      }

      Utils.Log($"[EffectModifier]: Initializing modifier {fxName}", LogType.Modifiers);
      var roots = parentEffect.GetModelTransforms();
      xforms = new();
      foreach (var t in roots)
      {
        var t1 = t.FindDeepChild(transformName);
        if (t1 == null)
        {
          Utils.LogError($"[EffectModifier]: Unable to find transform {transformName} on modifier {fxName}");
        }
        else
        {
          xforms.Add(t1);
        }
      }
    }

    public virtual ConfigNode Save()
    {
      var node = ConfigNode.CreateConfigFromObject(this);
      node.AddValue("name", fxName);
      node.AddValue("combinationType", effectMode.ToString());
      return node;
    }

    /// <summary>
    ///   Apply the effect with the various combine modes
    /// </summary>
    /// <param name="strength"></param>
    public virtual void Apply(float[] strength)
    {
      if (useRandomness && randomController != null)
      {
        float[] controllerData = randomController.Get();
        randomValue = controllerData[0] * randomnessScale;
      }

      switch (effectMode)
      {
        case EffectModifierMode.REPLACE:
          ApplyReplace(strength);
          break;
        case EffectModifierMode.ADD:
          ApplyAdd(strength);
          break;
        case EffectModifierMode.MULTIPLY:
          ApplyMultiply(strength);
          break;
        case EffectModifierMode.SUBTRACT:
          ApplySubtract(strength);
          break;
      }
    }

    protected virtual void ApplyReplace(float[] strength) { }

    protected virtual void ApplyAdd(float[] strength) { }

    protected virtual void ApplyMultiply(float[] strength) { }

    protected virtual void ApplySubtract(float[] strength) { }

    /// <summary>
    /// Returns true if this specific integrator is ideal for this modifier (ie EffectFloatIntegrator for an EffectFloatModifier and the associated transform matches
    /// </summary>
    /// <param name="integrator"></param>
    /// <returns></returns>
    public virtual bool IntegratorSuitable(EffectIntegrator integrator) => integrator is EffectIntegrator;

    public abstract EffectIntegrator CreateIntegrator();

    public virtual void CreateOrAttachToIntegrator<T>(List<T> integrators) where T : EffectIntegrator
    {
      if (integrators == null || !ValidForIntegrator) return;
      T target = integrators.FirstOrDefault(x => IntegratorSuitable(x));
      if (target == null)
      {
        target = CreateIntegrator() as T;
        integrators.Add(target);
      }
      else target.AddModifier(this);
      integrator = target;
    }

    public virtual void RemoveFromIntegrator<T>(List<T> integrators) where T : EffectIntegrator
    {
      if (integrators?.FirstOrDefault(x => x.handledModifiers.Contains(this)) is T integrator)
      {
        integrator.RemoveModifier(this);
        integrator = null;
      }
    }

    public void Get(float[] input, Vector3[] output, FloatCurve xCurve, FloatCurve yCurve, FloatCurve zCurve)
    {
      if (input.Length > 1)
      {
        for (int i = 0; i < xforms.Count; i++)
        {
          float inValue = input[i];
          output[i] = new(xCurve.Evaluate(inValue) + randomValue,
                          yCurve.Evaluate(inValue) + randomValue,
                          zCurve.Evaluate(inValue) + randomValue);
        }
      }
      else if (input.Length == 1)
      {
        float inValue = input[0];
        Vector3 vec = new(
          xCurve.Evaluate(inValue) + randomValue,
          yCurve.Evaluate(inValue) + randomValue,
          zCurve.Evaluate(inValue) + randomValue);
        for (int i = 0; i < xforms.Count; i++)
          output[i] = vec;
      }
    }
  }
}
