namespace Hms.Api.Domain
{
    public class UnitConversion : BaseEntity
    {
        public Guid UnitOfMeasureId { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; } = default!;

        /// <summary>
        /// Conversion unit name.
        /// Example: Kilogram
        /// </summary>
        public Guid SubUnitOfMeasureId { get; set; }

        public UnitOfMeasure SubUnitOfMeasure { get; set; } = default!;
        /// <summary>
        /// Quantity of sub units.
        /// Usually 1.
        /// Example:
        /// 1 KG
        /// </summary>
        public decimal SubUnitValue { get; set; } = 1;

        /// <summary>
        /// Quantity of base units.
        /// Example:
        /// 1000 G
        /// </summary>
        public decimal BaseUnitValue { get; set; }
    }
}
