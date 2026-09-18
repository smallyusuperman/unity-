using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkillConfig", menuName = "Game/Skills/Player Skill Config")]
public class PlayerSkillConfig : ScriptableObject
{
    [Min(0f)]
    [SerializeField] private float attackDamage = 30f;

    [Min(0f)]
    [SerializeField] private float attackInterval = 1f;

    [Min(0f)]
    [SerializeField] private float attackRange = 2f;

    public float AttackDamage => Mathf.Max(0f, attackDamage);
    public float AttackInterval => Mathf.Max(0f, attackInterval);
    public float AttackRange => Mathf.Max(0f, attackRange);
}
