using UnityEngine;

namespace DrillTool;

public class DrillTool : PlayerTool
{
	private static readonly int UseTool = Animator.StringToHash("drill");
	
    public Animator DrillAnimator;
    public Transform fxSpawnPoint;
    public VFXController fxControl;
    public FMOD_CustomLoopingEmitter Loop;
    public FMOD_CustomLoopingEmitter LoopHit;
    
	private bool isUsing;
    private GameObject activeHitObj;
    private Vector3 activeHitSpot;
    private Drillable activeDrillable;
    private LiveMixin activeLiveMixin;
    private float timeLastHit;
    private VFXSurfaceTypes prevSurfaceType = VFXSurfaceTypes.fallback;
    private AimIKTarget playerIKTarget;
    private ParticleSystem surfaceFxInstance;
    private GameObject lastHitObj;
    private float lostObjCooldown;
    
    public override string animToolName { get; } = TechType.Terraformer.AsString(true);
    public override bool GetUsedToolThisFrame() => isUsing;

    private void Start()
    {
	    playerIKTarget = Player.main.armsController.lookTargetTransform.GetComponent<AimIKTarget>();
    }

    private void OnDisable()
    {
	    TryDestroySurfaceVfx();
	    TryDrillableFx(false);
	    Loop.Stop();
	    LoopHit.Stop();
	    lostObjCooldown = 0f;
    }
    
    private void Update()
    {
	    isUsing = false;
	    
	    if (!isDrawn) return;
	    if (!usingPlayer) return;
	    
	    if (AvatarInputHandler.main.IsEnabled() && 
	        usingPlayer.IsAlive() && 
	        GameInput.GetButtonHeld(GameInput.Button.RightHand) && 
	        !usingPlayer.IsBleederAttached() &&
	        !energyMixin.IsDepleted())
	    {
		    isUsing = true;
	    }
	    
	    if (DrillAnimator)
	    {
		    DrillAnimator.SetBool(UseTool, isUsing);
	    }
	    
	    if (isUsing) Loop.Play();
	    else Loop.Stop();
	    
	    UpdateHitObj();
	    
	    if (isUsing && activeHitObj)
	    {
			UpdateHitBehaviorRefs();
		    LoopHit.Play();
		    
		    TryHit();

		    if (!activeDrillable)
		    {
			    ManageSurfaceVfx();
			    TryDrillableFx(false);
		    }
		    else
		    {
				TryDestroySurfaceVfx();
		    }
	    }
	    else
	    {
		    TryDestroySurfaceVfx();
		    TryDrillableFx(false);
		    LoopHit.Stop();
	    }
    }

    private void TryHit()
    {
	    if (Time.time <= timeLastHit + Plugin.ModConfig.DrillToolHitInterval) return;
	    timeLastHit = Time.time;
		    
	    if (activeDrillable)
	    {
		    //drill drillables. only this will consume energy
		    activeDrillable.OnDrill(activeHitSpot, null, out GameObject hitChunk);
		    energyMixin.ConsumeEnergy(Plugin.ModConfig.DrillToolEnergyCost);
		    
		    TryDrillableFx(true);
		    return;
	    }

	    if (activeLiveMixin)
	    {
		    //attack creatures
		    activeLiveMixin.TakeDamage(4f, activeHitSpot, DamageType.Drill);
		    return;
	    }

	    //break outcrops
	    activeHitObj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
    }

    //todo figure out IK, it works for laser cutter but not here
    public void SetTargetToClosest()
    {
	    Player.main.armsController.lookTargetTransform.position = activeHitSpot;
	    playerIKTarget.enabled = true;
    }
    
    private void UpdateHitBehaviorRefs()
    {
	    activeDrillable = activeHitObj.FindAncestor<Drillable>();
	    if (activeDrillable) return;

	    activeLiveMixin = activeHitObj.FindAncestor<LiveMixin>();
	    if (activeLiveMixin) return;
    }

    private void TryDrillableFx(bool enable)
    {
	    ParticleSystem fxPs = fxControl.emitters[0].fxPS;
	    if (!fxPs) return;
	    
	    if (enable && !fxPs.emission.enabled)
	    {
		    fxControl.Play(0);
		    
		    if (fxPs.main.scalingMode != ParticleSystemScalingMode.Hierarchy)
		    {
			    fxPs.transform.localScale = Vector3.one * 0.4f;
			    foreach (ParticleSystem ps in fxPs.GetComponentsInChildren<ParticleSystem>())
			    {
				    ParticleSystem.MainModule main = ps.main;
				    //main.startSizeMultiplier = 1f;
				    //main.startSpeedMultiplier = 1f;
				    main.gravityModifierMultiplier = 0.4f;
				    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
			    }
		    }
	    }
	    if (!enable && fxPs.emission.enabled)
	    {
		    fxControl.Stop(0);
	    }
    }

    private void ManageSurfaceVfx()
    {
	    //if we can hit a vfx surface, so the thing. support changing the vfx if the vfx material changes
	    VFXSurface vfxSurface = activeHitObj.GetComponent<VFXSurface>();

	    if (!surfaceFxInstance)
	    {
		    CreateSurfaceVfx(vfxSurface);
		    return;
	    }

	    if (vfxSurface && prevSurfaceType != vfxSurface.surfaceType)
	    {
		    TryDestroySurfaceVfx();
		    CreateSurfaceVfx(vfxSurface);
		    prevSurfaceType = vfxSurface.surfaceType;
	    }
    }
    
    private void UpdateHitObj()
    {
	    activeHitObj = null;
	    activeDrillable = null;
	    activeLiveMixin = null;
	    
	    UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 2.5f, ref activeHitObj, ref activeHitSpot, true);
	    
	    if (!activeHitObj)
	    {
		    InteractionVolumeUser interactVolume = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
		    if (interactVolume&& interactVolume.GetMostRecent() != null)
		    {
			    activeHitObj = interactVolume.GetMostRecent().gameObject;
		    }
	    }

	    //when we mine something, we won't lose track of it for a short while. but after that, it's actually lost.
	    //this is to solve sfx happening repeatedly due to how the TraceFPSTargetPosition is being odd when swimming around.
	    if (activeHitObj)
	    {
		    lastHitObj = activeHitObj;
		    lostObjCooldown = 0.1f;
	    }
	    else
	    {
		    lostObjCooldown -= Time.deltaTime;
		    if (lastHitObj && lostObjCooldown > 0)
		    {
			    //retain it
			    activeHitObj = lastHitObj;
		    }
	    } 
    }

    private void CreateSurfaceVfx(VFXSurface surface)
    {
	    if (surfaceFxInstance) Plugin.Logger.LogWarning("tried a drill CreateVfx when vfx already exists");
	    surfaceFxInstance = VFXSurfaceTypeManager.main.Play(surface, VFXEventTypes.exoDrill, fxSpawnPoint.position, fxSpawnPoint.rotation, fxSpawnPoint);
    }
    
    private void TryDestroySurfaceVfx()
    {
	    if (!surfaceFxInstance) return;
	    
	    surfaceFxInstance.GetComponent<VFXLateTimeParticles>().Stop();
	    Destroy(surfaceFxInstance.gameObject, 1.6f);
	    surfaceFxInstance = null;
    }

    /*private void OnGUI()
    {
	    //return;
        if (!isDrawn) return;
        if (!usingPlayer) return;
        
        int yPos = 20;
        GUI.Label(new Rect(20, yPos + 0, 300, 20), $"Used this frame: {isUsing}");
        GUI.Label(new Rect(20, yPos + 20, 300, 20), $"Active hit obj: {(activeHitObj ? "Yes" : "No")}");
        GUI.Label(new Rect(20, yPos + 40, 300, 20), $"Active drillable: {(activeDrillable ? "Yes" : "No")}");
        GUI.Label(new Rect(20, yPos + 60, 300, 20), $"Active live mixin: {(activeLiveMixin ? "Yes" : "No")}");
        GUI.Label(new Rect(20, yPos + 80, 300, 20), $"Drill spot: {activeHitSpot}");
    }*/
}