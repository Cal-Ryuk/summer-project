using UnityEngine;

[CreateAssetMenu(menuName = "COMBAT/Combo Data")]
public class ComboDataSO : ScriptableObject
{
    public ComboAttack[] attacks;
}

[System.Serializable]
public class ComboAttack
{
    public AnimationClip animClip;
    public float comboWindowDuration= 0.8f;
    public bool useRootMotion = true;
    public bool isShaking = false;
    public bool useHitStop = true;
    public float hitStopDuration = 0.2f;
}