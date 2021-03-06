using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Parsers
{
    public class AreaFilterParser : SimpleQueryParser
    {
        public AreaFilterParserVisitor.ValueHandlerDelegate
            ValueHandler { get; set; }

        public AreaFilterParserVisitor.IdHandlerDelegate
            IdHandler { get; set; }

        public AreaFilterParserVisitor.SpatialHandlerDelegate
            SpatialHandler { get; set; }

        public override string Parse(string inputString)
        {
            var visitor = new AreaFilterParserVisitor();
            visitor.ValueHandler = this.ValueHandler;
            visitor.IdHandler = this.IdHandler;
            visitor.SpatialHandler = this.SpatialHandler;

            return this.Parse(inputString, visitor);
        }
    }
}