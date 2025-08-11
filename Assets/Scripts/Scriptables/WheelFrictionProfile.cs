using System;
using Scriptables;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelFrictionProfile", menuName = "Vehicle/Wheel Friction")]
public class WheelFrictionProfile : ScriptableObject
{
    [Header("Front Wheel Forward")] [Tooltip("Kaymanın başladığı slip miktarı (küçük değer = daha erken kayar)")]
    public float frontForward_extremumSlip = 0.1f;

    [Tooltip("Kaymanın başladığı noktadaki sürtünme kuvveti")]
    public float frontForward_extremumValue = 1f;

    [Tooltip("Tam kayma durumundaki slip miktarı")]
    public float frontForward_asymptoteSlip = 0.8f;

    [Tooltip("Tam kayma durumundaki sürtünme kuvveti")]
    public float frontForward_asymptoteValue = 0.5f;

    [Tooltip("Genel sürtünme sertliği (düşük = kaygan, yüksek = tutucu)")]
    public float frontForward_stiffness = 1f;

    [Header("Front Wheel Sideways")] [Tooltip("Kaymanın başladığı slip miktarı (küçük değer = daha erken kayar)")]
    public float frontSideways_extremumSlip = 0.1f;

    [Tooltip("Kaymanın başladığı noktadaki sürtünme kuvveti")]
    public float frontSideways_extremumValue = 1f;

    [Tooltip("Tam kayma durumundaki slip miktarı")]
    public float frontSideways_asymptoteSlip = 0.5f;

    [Tooltip("Tam kayma durumundaki sürtünme kuvveti")]
    public float frontSideways_asymptoteValue = 0.95f;

    [Tooltip("Genel sürtünme sertliği (düşük = kaygan, yüksek = tutucu)")]
    public float frontSideways_stiffness = 1f;

    [Header("Back Wheel Forward")] [Tooltip("Kaymanın başladığı slip miktarı (küçük değer = daha erken kayar)")]
    public float backForward_extremumSlip = 0.1f;

    [Tooltip("Kaymanın başladığı noktadaki sürtünme kuvveti")]
    public float backForward_extremumValue = 1f;

    [Tooltip("Tam kayma durumundaki slip miktarı")]
    public float backForward_asymptoteSlip = 0.8f;

    [Tooltip("Tam kayma durumundaki sürtünme kuvveti")]
    public float backForward_asymptoteValue = 1f;

    [Tooltip("Genel sürtünme sertliği (düşük = kaygan, yüksek = tutucu)")]
    public float backForward_stiffness = 2f;

    [Header("Back Wheel Sideways")] [Tooltip("Kaymanın başladığı slip miktarı (küçük değer = daha erken kayar)")]
    public float backSideways_extremumSlip = 0.1f;

    [Tooltip("Kaymanın başladığı noktadaki sürtünme kuvveti")]
    public float backSideways_extremumValue = 1f;

    [Tooltip("Tam kayma durumundaki slip miktarı")]
    public float backSideways_asymptoteSlip = 0.5f;

    [Tooltip("Tam kayma durumundaki sürtünme kuvveti")]
    public float backSideways_asymptoteValue = 0.9f;

    [Tooltip("Genel sürtünme sertliği (düşük = kaygan, yüksek = tutucu)")]
    public float backSideways_stiffness = 2f;

    public WheelFrictionCurve GetWheelFrictionCurve(WheelType wheelType)
    {
        WheelFrictionCurve curve = new WheelFrictionCurve();

        switch (wheelType)
        {
            case WheelType.FrontForward:
                curve.extremumSlip = frontForward_extremumSlip;
                curve.extremumValue = frontForward_extremumValue;
                curve.asymptoteSlip = frontForward_asymptoteSlip;
                curve.asymptoteValue = frontForward_asymptoteValue;
                curve.stiffness = frontForward_stiffness;
                break;

            case WheelType.FrontSideways:
                curve.extremumSlip = frontSideways_extremumSlip;
                curve.extremumValue = frontSideways_extremumValue;
                curve.asymptoteSlip = frontSideways_asymptoteSlip;
                curve.asymptoteValue = frontSideways_asymptoteValue;
                curve.stiffness = frontSideways_stiffness;
                break;

            case WheelType.BackForward:
                curve.extremumSlip = backForward_extremumSlip;
                curve.extremumValue = backForward_extremumValue;
                curve.asymptoteSlip = backForward_asymptoteSlip;
                curve.asymptoteValue = backForward_asymptoteValue;
                curve.stiffness = backForward_stiffness;
                break;

            case WheelType.BackSideways:
                curve.extremumSlip = backSideways_extremumSlip;
                curve.extremumValue = backSideways_extremumValue;
                curve.asymptoteSlip = backSideways_asymptoteSlip;
                curve.asymptoteValue = backSideways_asymptoteValue;
                curve.stiffness = backSideways_stiffness;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(wheelType), wheelType, null);
        }

        return curve;
    }

}
