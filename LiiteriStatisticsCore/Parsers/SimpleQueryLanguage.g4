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
     | expr OR expr     # orExpression
     | expr AND expr    # andExpression
     | '(' expr ')'     # expressionExpression
     ;
 
relationalExpr : id (EQUALS|NOTEQUALS) value;

/* atom : value | ID; */
value : INT;
id : ID;


/*
 * Lexer Rules
 */
AND : 'AND';
OR : 'OR';
EQUALS : '=';
NOTEQUALS : '!=';
INT : [0-9]+;
ID : [a-zA-Z_]+;

WS
    :   (' ' | '\r' | '\n') -> channel(HIDDEN)
    ;
