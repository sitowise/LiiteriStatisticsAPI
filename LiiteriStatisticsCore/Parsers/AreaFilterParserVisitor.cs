using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace LiiteriStatisticsCore.Parsers
{
    public class AreaFilterParserVisitor : SimpleQueryLanguageBaseVisitor<string>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AreaFilterParserVisitor() : base()
        {
        }

        /* this is used by the original caller to push values to a
         * ParameterCollection, which will result with a parameterName,
         * which is then used here to build the actual piece of SQL query */
        public delegate string ValueHandlerDelegate(object value);
        public ValueHandlerDelegate ValueHandler = null;

        /* this is used by the original caller to map id field
         * to database field, which is then used here to build the actual
         * piece of SQL query*/
        public delegate string IdHandlerDelegate(string value);
        public IdHandlerDelegate IdHandler = null;

        public override string VisitValue(
            SimpleQueryLanguageParser.ValueContext context)
        {
            /* here we could have something other than INT,
             * either detect it here or this should be done in another
             * type specific method? */
            int value = int.Parse(context.INT().GetText());
            string paramName = this.ValueHandler(value);

            Debug.WriteLine(string.Format(
                "VisitValue, paramName={0}, value(retval)={1}",
                paramName, value));
            return paramName;
        }

        public override string VisitId(
            SimpleQueryLanguageParser.IdContext context)
        {
            string name = context.ID().GetText().ToString();
            string dbColumn = this.IdHandler(name);
            Debug.WriteLine(string.Format(
                "VisitId, will return {0}",
                dbColumn));
            return dbColumn;
        }

        public override string VisitRelationalExpr(
            SimpleQueryLanguageParser.RelationalExprContext context)
        {
            Debug.WriteLine("VisitRelationalExpr...");
            if (context.EQUALS() != null) {
                string left = Visit(context.id());
                string right = Visit(context.value());
                string retval = string.Format("{0} = {1}", left, right);
                Debug.WriteLine(string.Format(" will return {0}", retval));
                return retval;
            } else if (context.NOTEQUALS() != null) {
                string left = Visit(context.id());
                string right = Visit(context.value());
                string retval = string.Format("{0} <> {1}", left, right);
                Debug.WriteLine(string.Format(" will return {0}", retval));
                return retval;
            } else {
                throw new Exception("Error! Unhandled relational expression!");
            }
        }

        public override string VisitNullExpr(
            SimpleQueryLanguageParser.NullExprContext context)
        {
            if (context.ISNULL() != null) {
                string left = Visit(context.id());
                string retval = string.Format("{0} IS NULL", left);
                Debug.WriteLine(string.Format(" will return {0}", retval));
                return retval;
            } else {
                throw new Exception("Error! Unhandled null expression!");
            }
        }

        public override string VisitAndExpression(
            SimpleQueryLanguageParser.AndExpressionContext context)
        {
            string left = Visit(context.expr(0));
            string right = Visit(context.expr(1));
            string retval = string.Format(
                "({0} AND {1})",
                left, right);
            Debug.WriteLine(string.Format(
                "VisitAnd, will return {0}",
                retval));
            return retval;
        }

        public override string VisitOrExpression(
            SimpleQueryLanguageParser.OrExpressionContext context)
        {
            string left = Visit(context.expr(0));
            string right = Visit(context.expr(1));
            string retval = string.Format(
                "({0} OR {1})",
                left, right);
            Debug.WriteLine(string.Format(
                "VisitOr, will return {0}",
                retval));
            return retval;
        }

        public override string VisitNotExpression(
            SimpleQueryLanguageParser.NotExpressionContext context)
        {
            if (context.NOT() != null) {
                string expr = Visit(context.expr());
                string retval = string.Format("NOT ({0})", expr);
                Debug.WriteLine(string.Format(" will return {0}", retval));
                return retval;
            } else {
                throw new Exception("Error! Unhandled NOT expression!");
            }
        }

        public override string VisitExpressionExpression(
            SimpleQueryLanguageParser.ExpressionExpressionContext context)
        {
            string retval = string.Format("{0}", Visit(context.expr()));
            Debug.WriteLine(string.Format(
                "VisitParens, will return {0}",
                retval));
            return retval;
        }
    }
}