using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class SelectOptimizationStrategyRequestDto
    {
        public Guid RequestId { get; set; }
        public Guid SelectedStrategyId { get; set; }
    }
}
