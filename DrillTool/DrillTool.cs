using System;
using UnityEngine;

namespace DrillTool;

public class DrillTool : PlayerTool
{
    public Animator DrillAnimator;
    public Transform fxSpawnPoint;
    public VFXController fxControl;
    public FMOD_CustomLoopingEmitter Loop;
    public FMOD_CustomLoopingEmitter LoopHit;
    
	private bool isUsing;
    
    private GameObject activeHitObj;
    private GameObject prevHitObj;
    private Vector3 activeHitSpot;
    
    private Drillable activeDrillable;
    private LiveMixin activeLiveMixin;
    
    
    private float timeLastHit;
    private VFXSurfaceTypes prevSurfaceType = VFXSurfaceTypes.fallback;

    
    private AimIKTarget playerIKTarget;


    private ParticleSystem surfaceFxInstance;

    
	private static readonly int UseTool = Animator.StringToHash("drill");
    
    public override string animToolName { get; } = TechType.Terraformer.AsString(true);
    public override bool GetUsedToolThisFrame() => isUsing;

    private void Start()
    {
	    playerIKTarget = Player.main.armsController.lookTargetTransform.GetComponent<AimIKTarget>();
    }

    private void OnDisable()
    {
	    StopFx();
	    Loop.Stop();
	    LoopHit.Stop();
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
		    ManageSurfaceVfx();
	    }
	    else
	    {
		    StopFx();
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
	    if (!fxControl.emitters[0].fxPS) return;
	    
		fxControl.emitters[0].instanceGO.transform.localScale = Vector3.one * 0.1f;
		
	    if (enable && !fxControl.emitters[0].fxPS.emission.enabled)
	    {
		    //make it smaller scale because the tool itself is smaller
		    fxControl.Play(0);
	    }
	    if (!enable && fxControl.emitters[0].fxPS.emission.enabled)
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
    }
    
    
    private void StopFx()
    {
	    TryDestroySurfaceVfx();
	    TryDrillableFx(false);
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

    private void OnGUI()
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
    }
}