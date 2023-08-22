using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSuperstructureBlocks.Models
{
    public class FamilySymbolSelector
    {
        public string FamilyName { get; set; }
        public string SymbolName { get; set; }

        public FamilySymbolSelector(string familyName, string symbolName)
        {
            FamilyName = familyName;
            SymbolName = symbolName;
        }

        public override string ToString()
        {
            return $"{FamilyName} - {SymbolName}";
        }
    }
}
