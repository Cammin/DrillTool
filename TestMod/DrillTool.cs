using System;
using UnityEngine;

namespace TestMod;

internal class DrillTool : PlayerTool
{
    private bool usedThisFrame;
    
    private Drillable activeDrillable;
    private Vector3 activeDrillSpot;
    
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
    }
    
    public override void OnToolUseAnim(GUIHand hand)
    {
	    ErrorMessage.AddMessage("OnToolUseAnim");
        
    }
	
    public override void OnToolActionStart()
    {
	    ErrorMessage.AddMessage("OnToolActionStart");
	    
	    if (!activeDrillable) return;
	    
	    EnergyMixin battery = base.gameObject.GetComponent<EnergyMixin>();
	    if (battery.IsDepleted()) return;
		    
	    activeDrillable.OnDrill(activeDrillSpot, null, out var minedChunk);
	    battery.ConsumeEnergy(Plugin.ModConfig.DrillToolEnergyCost);
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
        if (!isDrawn) return;
        if (!usingPlayer) return;
        
        int yPos = 20;
        GUI.Label(new Rect(20, yPos, 300, 20), $"Used this frame: {usedThisFrame}");
        GUI.Label(new Rect(20, yPos + 20, 300, 20), $"Active drillable: {(activeDrillable ? "Yes" : "No")}");
        GUI.Label(new Rect(20, yPos + 40, 300, 20), $"Drill spot: {activeDrillSpot}");
    }
}