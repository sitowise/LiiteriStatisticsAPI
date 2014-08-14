grammar SimpleQueryLanguage;

/*
 * Parser Rules
 */

compileUnit
	:	EOF
	;

/*
 * Parser Rules
 */

prog: expr;

expr : relationalExpr   # relationalExpression
     | nullExpr         # nullExpression
     | NOT expr         # notExpression
     | expr AND expr    # andExpression
     | expr OR expr     # orExpression
     | '(' expr ')'     # expressionExpression
     ;
 
/*
sExpr : relationalExpr
      | nullExpr;
*/

relationalExpr : id (EQUALS|NOTEQUALS) value;
nullExpr : id ISNULL;

/* atom : value | ID; */
value : INT;
id : ID;


/*
 * Lexer Rules
 */
NOT : 'NOT';
AND : 'AND';
OR : 'OR';
EQUALS : '=';
NOTEQUALS : '<>';
ISNULL : ' IS NULL';
INT : [0-9]+;
ID : [a-zA-Z_]+;

WS
    :   (' ' | '\r' | '\n') -> channel(HIDDEN)
    ;
