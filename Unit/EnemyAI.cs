using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Personalities {
    Aggressive, //boosts attack cards
    Neutral, //default AI
    Defensive, //boosts defensive cards
    Conservative, //AI prefers to save action points for more costly moves
    Wreckless, //AI prefers to spend all AP
}

[System.Serializable]
public class EnemyAI : MonoBehaviour {

    [SerializeField]
    Personalities personality = Personalities.Neutral;

    [SerializeField]
    bool isSchizophrenic = false;

    [SerializeField]
    List<Move> enemyMoves = new List<Move>();

    [SerializeField]
    List<Target> Target = new List<Target>();

    [SerializeField]
    public float baseDoNothingMove = 20f;

    public void MoveSelection(Unit enemy, List<Unit> friendliesList) {
        return;
        List<Unit> friendlies = new List<Unit>();
        foreach(Unit friendly in friendliesList) {
            if(friendly.Health > 0) friendlies.Add(friendly);
        }
        this.Target.Clear();
        if(this.isSchizophrenic) this.personality = (Personalities)Random.Range(0, 5);
        this.getTargetModifiers(enemy, friendlies);
        if(this.Target.Count == 0) return;
        Move move = this.getMove(enemy, friendlies);
        if(move != null) enemy.addMove(move, null);
    }

    public void getTargetModifiers(Unit enemy, List<Unit> friendlies) {
        foreach(Unit friendly in friendlies) {
            List<Modifier> mods = new List<Modifier>();
            if(friendly.Health < (friendly.Health * 0.1f)) mods.Add(new Modifier(ModifierName.LowHP, friendly.Health));
            if(friendly.Health > (friendly.Health * 0.75f)) mods.Add(new Modifier(ModifierName.HighHP, friendly.Health));
            
            if(friendly.Speed > enemy.Speed) mods.Add(new Modifier(ModifierName.Faster, 1, true));
            if(friendly.Speed < enemy.Speed) mods.Add(new Modifier(ModifierName.Slower, 1));

            foreach(Effect effect in friendly.effects) {
                Modifier mod = new Modifier(ModifierName.Effect, 1);
                mod.effect = effect.type;
                switch(mod.effect) {
                    case EffectType.Resistance:
                    case EffectType.Shield:
                    case EffectType.Thorns:
                    case EffectType.Immunity:
                    case EffectType.Regen:
                        mod.inverted = true;
                        break;
                }
                mods.Add(mod);
            }

            foreach(Move move in friendly.moves) {
                foreach(Unit target in move.damageTargets) {
                    if(move.damage > target.Health) mods.Add(new Modifier(ModifierName.Kills, 5));
                }
            }
            if(!friendly.hasEffect(EffectType.Stealth)) this.Target.Add(new Target(friendly, mods));
        }
    }

    public Move getMove(Unit enemy, List<Unit> friendlies) {
        //Getting the moves the enemy can actually use
        List<Move> usableMoves = new List<Move>();

        foreach(Move move in this.enemyMoves) {
            if(
                move.moveCost.APcost <= enemy.ActionPoints &&
                move.moveCost.HPCost < enemy.Health &&
                move.moveCost.speedCost <= enemy.Speed
            ) {
                usableMoves.Add(move);
            }
        }

        List<MoveTargets> moveTargets = new List<MoveTargets>();
        
        foreach(Move move in usableMoves) {
            //Get all move values
            int ActionPointsLeft = enemy.ActionPoints - move.moveCost.APcost;
            int HPLeft = enemy.Health - move.moveCost.HPCost;
            int damageValue = move.damage;
            int isAOE = 0;
            if(move.damageTargetType == targetType.ALLENEMIES) isAOE = 1;
            int effectValue = 0;
            
            foreach(Effect effect in move.effects) {
                switch(effect.type) {
                    case EffectType.Poison:
                    case EffectType.Frail:
                    case EffectType.Slow:
                    case EffectType.Heartless:
                    case EffectType.Weak:
                    case EffectType.Confusion:
                    case EffectType.Exhaust:
                    case EffectType.Silence:
                    case EffectType.Stun:
                        if(effect.targets.Contains(enemy)) effectValue -= 1;
                        else effectValue += 1;
                        break;
                    case EffectType.Shield:
                    case EffectType.Protect:
                    case EffectType.Strength:
                    case EffectType.Regen:
                    case EffectType.Thorns:
                    case EffectType.Lifesteal:
                    case EffectType.Haste:
                    case EffectType.Immunity:
                    case EffectType.Endure:
                    case EffectType.Resistance:
                    case EffectType.Energized:
                        if(effect.targets.Contains(enemy)) effectValue += 1;
                        else effectValue -= 1;
                        break;
                    case EffectType.Oblivion:
                    case EffectType.Bomb:
                    case EffectType.Bound:
                        effectValue += 1;
                        break;
                    default: 
                        break;
                }
            }
            float moveWeight = ActionPointsLeft + HPLeft + (damageValue / 2) + isAOE + effectValue;
            //Get ideal target for the move
            MoveTargets idealTarget = null;
            foreach(Target um in this.Target) {
                float value = 0;
                if(damageValue > um.unit.Health) value += 15f;
                if(this.hasModifier(um.modifiers, ModifierName.Kills) != null) {
                    //Does this move have the effect to help said ally
                    // - Is the effect enough to protect ally
                    if(this.canSaveAlly(move.effects, um.unit.moves)) {
                        value += 25f;
                    }
                }
                value += (um.unit.Health + um.unit.ActionPoints) / 10; //TODO: add more differentiating values
                if(idealTarget != null) {
                    if(idealTarget.value < value) idealTarget = new MoveTargets(move, um.unit, value);
                } else {
                    idealTarget = new MoveTargets(move, um.unit, value);
                }
            }
            move.damageTargets.Add(idealTarget.target);

            //Ideal target found, now add move weight to target value
            idealTarget.value += moveWeight;
            //Personalities
            if(this.personality == Personalities.Aggressive) idealTarget.value += (damageValue / 2);
            if(this.personality == Personalities.Defensive) {
                int defensiveValue = 0;
                foreach(Effect effect in move.effects) {
                    switch(effect.type) {
                        case EffectType.Protect:
                        case EffectType.Resistance:
                        case EffectType.Regen:
                        case EffectType.Endure:
                        case EffectType.Immunity:
                        case EffectType.Lifesteal:
                        case EffectType.Thorns:
                            defensiveValue += 1;
                            break;
                    }
                }
                idealTarget.value += defensiveValue;
            }
            moveTargets.Add(idealTarget);
        }

        //Actually choose move
        float totalValue = 0;
        float doNothing = this.baseDoNothingMove;
        if(this.personality == Personalities.Conservative) doNothing += 1f;
        if(this.personality == Personalities.Wreckless) doNothing -= 1f;
        moveTargets.Add(new MoveTargets(null, null, doNothing));
        foreach(MoveTargets target in moveTargets) {
            totalValue += target.value;
        }

        float randomNumber = Random.Range(0, totalValue);
        float floor = 0;

        for(int i = 0; i < moveTargets.Count; i++) {
            if(randomNumber < (floor + moveTargets[i].value)) {
                //Found the choice!
                return moveTargets[i].move;
            } else {
                floor += moveTargets[i].value;
            }
        }

        return null; //Just because the compiler is too stupid to actually realize we are returning
    }

    public Modifier hasModifier(List<Modifier> modifiers, ModifierName name) {
        foreach(Modifier mod in modifiers) {
            if(mod.modifierName == name) return mod;
        }
        return null;
    }

    public bool canSaveAlly(List<Effect> effects, List<Move> moves) {
        foreach(Move move in moves) {
            foreach(Unit target in move.damageTargets) {
                if(move.damage > target.Health) {
                    foreach(Effect effect in effects) {
                        if(effect.type == EffectType.Resistance && move.damage / 2 < target.Health) return true;
                        if(effect.type == EffectType.Stealth) return true;
                        if(effect.type == EffectType.Shield && move.damage < target.Health + effect.stackCount) return true;
                        if(effect.type == EffectType.Protect) return true;
                        if(effect.type == EffectType.Endure) return true;
                    }
                }
            }
        }
        return false;
    }
}

[System.Serializable]
public class MoveTargets {
    public MoveTargets(Move move, Unit target, float value) {
        this.move = move;
        this.target = target;
        this.value = value;
    }
    public Move move;
    public Unit target;
    public float value;
}

[System.Serializable]
public class Target {
    public Target(Unit unit, List<Modifier> mods) {
        this.unit = unit;
        this.modifiers = mods;
    }
    [SerializeField]
    public Unit unit;
    [SerializeField]
    public List<Modifier> modifiers = new List<Modifier>();    
}

public enum ModifierName {
    LowHP,
    HighHP,
    Faster,
    Slower,
    Effect,
    Kills
}

[System.Serializable]
public class Modifier {

    public Modifier(ModifierName name, int value, bool inverted = false) {
        this.modifierName = name;
        this.value = value;
        this.inverted = inverted;
    }

    [SerializeField]
    public ModifierName modifierName;

    [SerializeField]
    public EffectType effect;

    [SerializeField]
    public int value;

    [SerializeField]
    public bool inverted;

}
