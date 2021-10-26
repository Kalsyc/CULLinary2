using UnityEngine;


[CreateAssetMenu(fileName = "New Skill Item", menuName = "Weapon Skill/Skill Item")]
public class SkillItem : WeaponSkillItem
{
    public int[] blockPercentage;
    public int[] staminaCost;
    public int[] skillDuration;
    public int[] attackDamage;
    public GameObject skillPrefab;

    public string GetLevelDescription(int level)
    {
        string damageDesc = "Damage: " + attackDamage[level] + "DMG";
        string staminaCostDesc = "Stamina Cost: " + staminaCost[level];
        return damageDesc + "\n" + staminaCostDesc;
    }

}
