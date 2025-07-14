using Nautilus.Assets.PrefabTemplates;
using Nautilus.Extensions;
using UnityEngine;
using UWE;

namespace TestMod.Items;

internal class HandDrill : PlayerTool
{
    private bool usedThisFrame;
    private Drillable activeDrillTarget;
    
    public override void OnToolUseAnim(GUIHand hand)
    {
        base.OnToolUseAnim(hand);

        TryDamageRock();
    }

    private void TryDamageRock()
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

    private void Update()
    {
        usedThisFrame = false;
        if (!isDrawn) return;
        if (AvatarInputHandler.main.IsEnabled() && Player.main.IsAlive() && Player.main.GetRightHandHeld() && !Player.main.IsBleederAttached())
        {
            usedThisFrame = true;
            Debug.Log("Using!");
        }
        
        //this.UpdateTarget();
        this.UpdateUI();
    }
    
    private void UpdateUI()
    {
        /*if (this.activeDrillTarget != null)
        {
            float healthFraction = this.activeDrillTarget.health();
            if (healthFraction < 1f)
            {
                HandReticle main = HandReticle.main;
                main.SetProgress(healthFraction);
                main.SetText(HandReticle.TextType.Hand, "Weld", true, GameInput.Button.None);
                main.SetIcon(HandReticle.IconType.Progress, 1.5f);
            }
        }*/
    }

    public override bool GetUsedToolThisFrame()
    {
        return usedThisFrame;
    }

    public override void OnToolActionStart()
    {
        base.OnToolActionStart();

        Vector3 zero = Vector3.zero;
        GameObject obj = null;
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
}