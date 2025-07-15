using UnityEngine;

namespace TestMod;

internal class DrillTool : PlayerTool
{
    private bool usedThisFrame;
    private Drillable activeDrillTarget;
    
    private void OnDisable()
    {
	    activeDrillTarget = null;
    }
    
    private void Update()
    {
	    usedThisFrame = false;
	    if (!isDrawn) return;
	    if (AvatarInputHandler.main.IsEnabled() && Player.main.IsAlive() && Player.main.GetRightHandHeld() && !Player.main.IsBleederAttached())
	    {
		    usedThisFrame = true;
	    }
        
	    //UpdateTarget();
    }
    
    //use
    public override void OnToolUseAnim(GUIHand hand)
    {
        Plugin.Logger.LogInfo("Trying damage rock");
        if (this.activeDrillTarget == null) return;
        
        EnergyMixin component = base.gameObject.GetComponent<EnergyMixin>();
        if (!component.IsDepleted())
        {
	        activeDrillTarget.OnDrill(activeDrillTarget.transform.position, null, out GameObject hitObj);
	        component.ConsumeEnergy(2);
        }
    }

    public override bool GetUsedToolThisFrame()
    {
        return usedThisFrame;
    }

    //from exosuit arm code
    public override void OnToolActionStart()
    {
        base.OnToolActionStart();

        Vector3 zero = Vector3.zero;
        GameObject obj = null;
        //TraceForTarget()
        UWE.Utils.TraceFPSTargetPosition(Player.mainObject, 5f, ref obj, ref zero, true);

        if (obj)
        {
            Drillable drillable = obj.FindAncestor<Drillable>();
            
            if (!drillable)
            {
                obj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
                return;
            }
            
            //drillable.OnDrill(zero, null, out hitObj);
        }
    }
    
    //exosuit drill arm code
    public void OnHit()
	{
		if (this.exosuit.CanPilot() && this.exosuit.GetPilotingMode())
		{
			Vector3 zero = Vector3.zero;
			GameObject gameObject = null;
			this.drillTarget = null;
			global::UWE.Utils.TraceFPSTargetPosition(this.exosuit.gameObject, 5f, ref gameObject, ref zero, true);
			if (gameObject == null)
			{
				InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
				if (component != null && component.GetMostRecent() != null)
				{
					gameObject = component.GetMostRecent().gameObject;
				}
			}
			if (gameObject && this.drilling)
			{
				Drillable drillable = gameObject.FindAncestor<Drillable>();
				this.loopHit.Play();
				if (!drillable)
				{
					LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
					if (liveMixin)
					{
						liveMixin.IsAlive();
						liveMixin.TakeDamage(4f, zero, DamageType.Drill, null);
						this.drillTarget = gameObject;
					}
					VFXSurface component2 = gameObject.GetComponent<VFXSurface>();
					if (this.drillFXinstance == null)
					{
						this.drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, this.vfxEventType, this.fxSpawnPoint.position, this.fxSpawnPoint.rotation, this.fxSpawnPoint);
					}
					else if (component2 != null && this.prevSurfaceType != component2.surfaceType)
					{
						this.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
						global::UnityEngine.Object.Destroy(this.drillFXinstance.gameObject, 1.6f);
						this.drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, this.vfxEventType, this.fxSpawnPoint.position, this.fxSpawnPoint.rotation, this.fxSpawnPoint);
						this.prevSurfaceType = component2.surfaceType;
					}
					gameObject.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
					return;
				}
				GameObject gameObject2;
				drillable.OnDrill(this.fxSpawnPoint.position, this.exosuit, out gameObject2);
				this.drillTarget = gameObject2;
				if (this.fxControl.emitters[0].fxPS != null && !this.fxControl.emitters[0].fxPS.emission.enabled)
				{
					this.fxControl.Play(0);
					return;
				}
			}
			else
			{
				this.StopEffects();
			}
		}
	}
}