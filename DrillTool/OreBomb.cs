using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using Nautilus.Utility;
using ProtoBuf;
using UnityEngine;
using UWE;

namespace DrillTool;

public class OreBomb : PlayerTool
{
    public GameObject detonateParticlePrefab;
    
    [NonSerialized]
    [ProtoMember(1)]
    public float fuseLifetime = 3f;
	
    [NonSerialized]
    [ProtoMember(2)]
    public bool isPrimed;

    [NonSerialized]
    [ProtoMember(4)]
    public float flareActivateTime;
    
    public FMOD_CustomEmitter inflateSound;
    private bool exploded;
    
    
	public Light light;
	public FMOD_CustomLoopingEmitter loopingSound;
	public FMOD_StudioEventEmitter throwSound;
	public VFXController fxControl;
	public MeshRenderer capRenderer;
	public Rigidbody useRigidbody;

	private bool fxIsPlaying;
	private bool isThrowing;
	private float originalIntensity;
	private float originalrange;
	private Sequence sequence;
	
	public override string animToolName { get; } = TechType.Flare.AsString(true);

	public override void Awake()
	{
		base.Awake();
		light.intensity = 1f;
		light.range = 10;
		
		originalIntensity = light.intensity;
		originalrange = light.range;
		light.intensity = 0f;
		light.range = 0f;
		sequence = new Sequence();
		
		if (isPrimed)
		{
			StartPersistentStateStuff();
		}
	}

	private void UpdateLight()
	{
		float lifetime = (float)DayNightCycle.main.timePassed - flareActivateTime;
		if (lifetime > 0.1f)
		{
			float num2 = lifetime / 0.25f;
			float num3 = 0.45f + 0.55f * Mathf.PerlinNoise(num2, 0f);
			float num4 = originalIntensity * num3;
			float num5 = originalrange * 0.65f + 0.35f * Mathf.Sin(num2);
			if (lifetime < 0.43f)
			{
				float t = lifetime * 3f - 0.1f;
				float intensity = Mathf.Lerp(0f, num4, t);
				FlashingLightHelpers.SafeIntensityChangePerFrame(light, intensity);
				float range = Mathf.Lerp(0f, num5, t);
				FlashingLightHelpers.SafeRangeChangePreFrame(light, range);
			}
			else
			{
				FlashingLightHelpers.SafeIntensityChangePerFrame(light, num4);
				FlashingLightHelpers.SafeRangeChangePreFrame(light, num5);
			}
		}
	}

	private void Update()
	{
		if (exploded) return;

		if (isThrowing)
		{
			sequence.Update();
		}
		
		if (isPrimed)
		{
			UpdateLight();
			fuseLifetime = Mathf.Max(fuseLifetime - Time.deltaTime, 0f);
		}
		
		if (fuseLifetime <= 0f)
		{
			loopingSound.Stop();
			Detonate();
		}
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.IsBleederAttached())
		{
			return true;
		}
		_isInUse = true;
		return true;
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		if (isThrowing) return;
		isThrowing = true;
		
		sequence.Set(0.5f, target: true, Throw);
	}

	private void Throw()
	{
		isThrowing = false;
		_isInUse = false;
		isPrimed = true;
		
		flareActivateTime = DayNightCycle.main.timePassedAsFloat;
		fuseLifetime = 3;
		
		StartPersistentStateStuff();
		
		throwSound.StartEvent();
		pickupable.Drop(transform.position);
	}

	public void StartPersistentStateStuff()
	{
		useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		pickupable.isPickupable = false;
		transform.GetComponent<WorldForces>().enabled = true;
		
		capRenderer.enabled = false;
		light.enabled = true;
		loopingSound.Play();
		
		if (fxControl && !fxIsPlaying)
		{
			fxIsPlaying = true;
			fxControl.Play(1);
		}
	}
	
	private void OnDrop()
	{
		if (isPrimed)
		{
			useRigidbody.AddForce(MainCamera.camera.transform.forward * 100f);
			useRigidbody.AddTorque(transform.right * 5f);
		}
	}

	private void OnDisable()
	{
		if (!fxIsPlaying) return;
		fxIsPlaying = false;
		
		fxControl.StopAndDestroy(1, 0f);
	}

	public override void OnDestroy()
	{
		fxControl.StopAndDestroy(2f);
		base.OnDestroy();
	}
    
	private void Detonate()
	{
		exploded = true;
		if (detonateParticlePrefab)
		{
			Utils.PlayOneShotPS(detonateParticlePrefab, transform.position, transform.rotation);
		}
		DamageSystem.RadiusDamage(50, transform.position, 5, DamageType.Explosive, gameObject);
		RadiusDamageDrillable(transform.position, 5);
		
		FMODAsset asset = AudioUtils.GetFmodAsset("event:/creature/crash/die");
		Utils.PlayFMODAsset(asset, transform.position);
		
		Destroy(gameObject, 2f);
	}
    
    
	public void RadiusDamageDrillable(Vector3 position, float detonateRadius)
	{
		int count = UWE.Utils.OverlapSphereIntoSharedBuffer(position, detonateRadius);

		HashSet<BreakableResource> hitBreakables = new HashSet<BreakableResource>();
		HashSet<Drillable> hitDrillables = new HashSet<Drillable>();
		
		for (int i = 0; i < count; i++)
		{
			BreakableResource outcrop = Utils.FindAncestorWithComponent<BreakableResource>(UWE.Utils.sharedColliderBuffer[i].gameObject);
			if (outcrop && hitBreakables.Add(outcrop))
			{
				outcrop.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
				continue;
			}
			
			Drillable drillable = Utils.FindAncestorWithComponent<Drillable>(UWE.Utils.sharedColliderBuffer[i].gameObject);
			if (drillable && hitDrillables.Add(drillable))
			{
				DrillablePatcher.Crumble(drillable);
			}
		}
	}
}