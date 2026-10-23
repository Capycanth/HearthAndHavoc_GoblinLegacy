using HearthAndHavoc_GoblinLegacy.Enumeration;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Motivation
{
    // Something a creature wants, which the decision layer turns into chain scores (Milestone 7, decision 8). Level
    // says how pressing it is, from 0 (fully met) to 1 (as pressing as it gets), and UrgencyExponent shapes how
    // sharply the score climbs as the level rises.
    public abstract class Need
    {
        // The exponent every body need (hunger, thirst and sleep) uses.
        protected const float BodyUrgencyExponent = 1.5f;

        public NeedType Type { get; }
        public float UrgencyExponent { get; }

        // Clamped, so float rounding (a stomach a hair over capacity, say) never gives a level outside 0 to 1. A
        // negative level raised to a fractional exponent during scoring would be NaN.
        public float Level => Math.Clamp(CalculateLevel(), 0f, 1f);

        protected Need(NeedType type, float urgencyExponent)
        {
            Type = type;
            UrgencyExponent = urgencyExponent;
        }

        protected abstract float CalculateLevel();
    }
}
