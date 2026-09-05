using UnityEngine;

namespace MergeWars.Economy
{
    // TODO(design): This is a minimal stub covering only the interface
    // Generator needs (CanAfford/Spend). No SYS_ManaEconomy.md exists yet
    // in /GameDocs/Systems/ — replace with the real ManaEconomy
    // implementation per that doc once it's written. Starting mana value
    // and the absence of any regen/cap behavior are ASSUMED PLACEHOLDERS,
    // not confirmed design values.
    public class ManaEconomy : MonoBehaviour
    {
        [Tooltip("ASSUMED PLACEHOLDER starting mana value — not confirmed anywhere in /GameDocs/.")]
        [SerializeField] private int currentMana = 100;

        public int CurrentMana => currentMana;

        public bool CanAfford(int cost)
        {
            return currentMana >= cost;
        }

        public bool Spend(int cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            currentMana -= cost;
            return true;
        }
    }
}
