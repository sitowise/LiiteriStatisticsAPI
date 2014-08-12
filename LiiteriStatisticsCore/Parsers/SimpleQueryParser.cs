using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace LiiteriStatisticsCore.Parsers
{
    public abstract class SimpleQueryParser
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public abstract string Parse(string inputString);

        public string Parse(
            string inputString,
            ISimpleQueryLanguageVisitor<string> visitor)
        {
            logger.Debug(string.Format(
                "Parse inputString: {0}", inputString));
            Debug.WriteLine(string.Format(
                "Parse inputString: {0}", inputString));

            using (MemoryStream inputStream =
                    new MemoryStream(Encoding.ASCII.GetBytes(inputString))) {
                inputStream.Position = 0;

                AntlrInputStream input = new AntlrInputStream(inputStream);

                SimpleQueryLanguageLexer lexer =
                    new SimpleQueryLanguageLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                SimpleQueryLanguageParser parser =
                    new SimpleQueryLanguageParser(tokens);
                parser.ErrorHandler = new BailErrorStrategy();

                IParseTree tree = parser.prog();
                Debug.WriteLine(string.Format("ParseTree: {0}",
                    tree.ToStringTree(parser)));
                logger.Debug(string.Format("ParseTree: {0}",
                    tree.ToStringTree(parser)));

                string result = visitor.Visit(tree);
                logger.Debug(string.Format("Parse result: {0}", result));
                Debug.WriteLine(string.Format("Parse result: {0}", result));
                return result;
            }
        }
    }
}