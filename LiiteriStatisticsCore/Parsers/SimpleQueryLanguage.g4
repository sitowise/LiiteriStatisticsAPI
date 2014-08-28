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
     | spatialExpr      # spatialExpression
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

spatialAtom: ID | STRING;
spatialExpr: spatialAtom (SEQUALS|DISJOINT|INTERSECTS|TOUCHES|CROSSES|WITHIN|CONTAINS|OVERLAPS) spatialAtom;

/*
 * Lexer Rules
 */
NOT : 'NOT';
AND : 'AND';
OR : 'OR';
EQUALS : '=';
NOTEQUALS : '<>';
ISNULL : ' IS NULL';

/* BBOX: 'BBOX'; */
SEQUALS: 'EQUALS';
DISJOINT: 'DISJOINT';
INTERSECTS: 'INTERSECTS';
TOUCHES: 'TOUCHES';
CROSSES: 'CROSSES';
WITHIN: 'WITHIN';
CONTAINS: 'CONTAINS';
OVERLAPS: 'OVERLAPS';

INT : [0-9]+;
ID : [a-zA-Z_][a-zA-Z_0-9]*;
STRING :  '\'' ( ~'\'' | '\'\'' )* '\'';

WS
    :   (' ' | '\r' | '\n') -> channel(HIDDEN)
    ;
