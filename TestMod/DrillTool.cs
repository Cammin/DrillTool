using System;
using UnityEngine;

namespace DrillTool;

public class DrillTool : PlayerTool
{
    private bool usedThisFrame;
    
    private Drillable activeDrillable;
    private Vector3 activeDrillSpot;
    
    private float timeLastHit;
    
    public override bool GetUsedToolThisFrame() => usedThisFrame;
    
    private void OnDisable()
    {
	    activeDrillable = null;
    }
    
    private void Update()
    {
	    usedThisFrame = false;
	    if (!isDrawn) return;
	    if (!usingPlayer) return;
	    
	    if (AvatarInputHandler.main.IsEnabled() && usingPlayer.IsAlive() && usingPlayer.GetRightHandHeld() && !usingPlayer.IsBleederAttached())
	    {
		    usedThisFrame = true;
	    }
		UpdateTarget();
		TryHitDrillable();
    }

    private void TryHitDrillable()
    {
	    if (!usedThisFrame) return;
	    if (!activeDrillable) return;
	    
	    if (Time.time > timeLastHit + Plugin.ModConfig.DrillToolHitInterval)
	    {
		    EnergyMixin battery = base.gameObject.GetComponent<EnergyMixin>();
		    if (battery.IsDepleted()) return;
		    
		    activeDrillable.OnDrill(activeDrillSpot, null, out var minedChunk);
		    
		    battery.ConsumeEnergy(Plugin.ModConfig.DrillToolEnergyCost);
		    timeLastHit = Time.time;
		    
		    //ErrorMessage.AddMessage($"OnDrill {minedChunk.name}");
	    }
    }



    /*
    public override void OnToolUseAnim(GUIHand hand)
    {
	    ErrorMessage.AddMessage("OnToolUseAnim");
        
    }

    public override void OnDraw(Player p)
    {
	    base.OnDraw(p);
	    ErrorMessage.AddMessage("OnDraw");
    }

    public override void OnHolster()
    {
	    base.OnHolster();
	    ErrorMessage.AddMessage("OnHolster");
    }

    public override void OnToolBleederHitAnim(GUIHand guiHand)
    {
	    base.OnToolBleederHitAnim(guiHand);
	    ErrorMessage.AddMessage("OnToolBleederHitAnim");
    }

    public override void OnToolReloadBeginAnim(GUIHand guiHand)
    {
	    base.OnToolReloadBeginAnim(guiHand);
	    ErrorMessage.AddMessage("OnToolReloadBeginAnim");
    }

    public override void OnToolReloadEndAnim(GUIHand guiHand)
    {
	    base.OnToolReloadEndAnim(guiHand);
	    ErrorMessage.AddMessage("OnToolReloadEndAnim");
    }

    public override void OnToolAnimHolster()
    {
	    base.OnToolAnimHolster();
	    ErrorMessage.AddMessage("OnToolAnimHolster");
    }

    public override void OnToolAnimDraw()
    {
	    base.OnToolAnimDraw();
	    ErrorMessage.AddMessage("OnToolAnimDraw");
    }

    public override void OnFirstUseAnimationStop()
    {
	    base.OnFirstUseAnimationStop();
	    ErrorMessage.AddMessage("OnFirstUseAnimationStop");
    }*/

    public override void OnToolActionStart()
    {
	    base.OnToolActionStart();
	    //ErrorMessage.AddMessage("OnToolActionStart");
	    
	    
    }
    
    private void UpdateTarget()
    {
	    activeDrillable = null;
	    
	    if (usingPlayer == null) return;
	    
	    GameObject hitObj = null;
	    
	    UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 2f, ref hitObj, ref activeDrillSpot, true);
	    if (hitObj == null)
	    {
		    InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
		    if (component != null && component.GetMostRecent() != null)
		    {
			    hitObj = component.GetMostRecent().gameObject;
		    }
	    }

	    if (!hitObj) return;
	    
	    Drillable drillable = hitObj.FindAncestor<Drillable>();
	    if (drillable)
	    {
			activeDrillable = drillable;
	    }
    }

    private void OnGUI()
    {
	    return;
        if (!isDrawn) return;
        if (!usingPlayer) return;
        
        int yPos = 20;
        GUI.Label(new Rect(20, yPos, 300, 20), $"Used this frame: {usedThisFrame}");
        GUI.Label(new Rect(20, yPos + 20, 300, 20), $"Active drillable: {(activeDrillable ? "Yes" : "No")}");
        GUI.Label(new Rect(20, yPos + 40, 300, 20), $"Drill spot: {activeDrillSpot}");
    }
}