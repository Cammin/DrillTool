using System.Collections;
using UnityEngine;

namespace DrillTool;

public class DrillToolFragment : MonoBehaviour
{
    public EnergyEffect energyEffect;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        energyEffect.powerDown.Update(false);
        energyEffect.powerScalar.Update(0);
    }
}