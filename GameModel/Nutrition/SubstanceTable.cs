using HearthAndHavoc_GoblinLegacy.Enumeration;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Nutrition
{
    public static class SubstanceTable
    {
        public static Enzyme GetEnzyme(Substance substance) => substance switch
        {
            Substance.SIMPLE_CARB => Enzyme.AMYLASE,
            Substance.CELLULOSE => Enzyme.CELLULASE,
            Substance.CHITIN => Enzyme.CHITINASE,
            Substance.COMPLEX_SUGAR => Enzyme.GALACTOSIDASE,
            Substance.PROTEIN => Enzyme.PROTEASE,
            Substance.FAT => Enzyme.LIPASE,
            Substance.KERATIN => Enzyme.KERATINASE,
            _ => throw new ArgumentOutOfRangeException(nameof(substance), substance, "Substance has no enzyme in the table.")
        };

        public static float GetKcalPerGram(Substance substance) => substance switch
        {
            Substance.SIMPLE_CARB => 4f,
            Substance.CELLULOSE => 4f,
            Substance.CHITIN => 4f,
            Substance.COMPLEX_SUGAR => 4f,
            Substance.PROTEIN => 4f,
            Substance.FAT => 9f,
            Substance.KERATIN => 4f,
            _ => throw new ArgumentOutOfRangeException(nameof(substance), substance, "Substance has no energy density in the table.")
        };
    }
}
