grammar BoardGame;

options {
    language=CSharp3;
    output=AST;
	backtrack=true;
}

tokens {
	ASSIGNMENT;
	EVENT;
	EVENTS;
	EVENTTYPE;
	EVENTTYPES;
	FUNCCALL;
	FUNCDEF;
	FUNCDEFLIST;
	FUNCNAME;
	IF;
	IF_CONDITION;
	IF_ACTION;
	INIT;
	LIST;
	LIT_COORDS;
	LIT_INT;
	LIT_SET;
	MEMBER_ACCESS;
	MOVE_FROM;
	MOVE_OPTIONS;
	MOVE_TO;
	MOVES;
	ONLY_MODIFIER;
	OP_MOVE;
	PARAMLIST;
	PLAYERREF;
	REF;
	SELECT;
	SELECT_FROM;
	SELECT_WHERE;
	SETTINGS;
	STARTINGPIECES;
	STARTINGBOARD;
	STATEMENTS;
	TAG;
	VAR_REF;
	VAR_MEMBER_ACCESS;
}

@namespace { Level14.BoardGameRules }


// Lexer rules:

OP_ADD: '+';
OP_AND: 'And';
OP_ASSIGNMENT: ':=';
OP_DIV: '/';
OP_EQ: '=';
OP_GT: '>';
OP_GTE: '>=';
OP_LT: '<';
OP_LTE: '<=';
OP_MOD: '%';
OP_MUL: '*';
OP_NE: '!=';
OP_NOT: 'Not';
OP_OR: 'Or';
OP_SUB: '-';

OFFBOARD: 'Offboard';

NAME: ('a'..'z'|'A'..'Z'|'$') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')* ;

PLACEHOLDER: '_';

INT: '0'..'9'+ ;

WS: ( ' ' | '\t' | '\r' | '\n' )
    {$channel=Antlr.Runtime.TokenChannels.Hidden;} ;

Comment: '#' ~( '\r' | '\n' )*
	{$channel=Antlr.Runtime.TokenChannels.Hidden;} ;


// Parser rules:

int: INT -> ^(LIT_INT INT);
name:
	OFFBOARD -> ^(REF OFFBOARD) |
	NAME -> ^(REF NAME);
varName:
	NAME -> ^(VAR_REF NAME);

ref:
	playerRef
	| namedFunc
	| name;

intRef: int | ref;

// Expressions:

coord: '{' placeholderExpr (',' placeholderExpr )* '}' -> ^(LIT_COORDS placeholderExpr+);
playerRef: 'Player' '(' expr ')' -> ^(PLAYERREF expr);

selectExpr: '[' 'Select' name 'From' ref ( 'Where' expr )? ']' -> ^(SELECT name ^(SELECT_FROM ref) ^(SELECT_WHERE expr));
setLiteral: '[' (expr (',' expr)* ','?)? ']' -> ^(LIT_SET expr+);
setExpr: selectExpr | setLiteral;

namedFunc: name '(' (expr (',' expr)*)? ')' -> ^(FUNCCALL ^(FUNCNAME name) ^(LIST expr*));

expr: andExpr (OP_OR^ andExpr)*;
andExpr: eqExpr (OP_AND^ eqExpr)*;
eqExpr: relExpr ((OP_EQ|OP_NE)^ relExpr)*;
relExpr: addExpr ((OP_GT|OP_LT|OP_GTE|OP_LTE)^ addExpr)*;
addExpr: mulExpr ((OP_ADD|OP_SUB)^ mulExpr)*;
mulExpr: unaryExpr ((OP_MUL|OP_DIV|OP_MOD)^ unaryExpr)*;
unaryExpr:
	OP_SUB memberExpr -> ^(OP_SUB memberExpr) |
	OP_ADD? memberExpr -> memberExpr |
	OP_NOT memberExpr -> ^(OP_NOT memberExpr);

memberExpr:
	primeExpr ('.' nameOrFunction)+ -> ^(MEMBER_ACCESS primeExpr nameOrFunction*) |
	primeExpr;

varMemberExpr:
	primeExpr ('.' nameOrFunction)* '.' varName -> ^(MEMBER_ACCESS primeExpr nameOrFunction varName);

nameOrFunction: namedFunc | name;

methodStatement:
	primeExpr ('.' nameOrFunction)* '.' namedFunc -> ^(MEMBER_ACCESS primeExpr nameOrFunction* namedFunc);

primeExpr: int | coord | ref | setExpr | '(' expr ')' -> expr;
placeholderExpr: '_' | expr;

assignmentStatement:
	varMemberExpr OP_ASSIGNMENT expr -> ^(ASSIGNMENT varMemberExpr expr) |
	varName OP_ASSIGNMENT expr -> ^(ASSIGNMENT varName expr);

ifBlock:
	'If' expr 'Then' statement+ 'End' -> ^(IF ^(IF_CONDITION expr) ^(IF_ACTION statement+) );

returnStatement:
	'Return' expr -> ^('Return' expr);

statement:
	assignmentStatement ';' -> assignmentStatement |
	returnStatement ';' -> returnStatement |
	methodStatement ';' -> methodStatement |
	ifBlock |
	namedFunc ';' -> namedFunc ;

settingsRow:
	(
	  NAME ':' int -> ^(NAME int)
	| NAME ':' coord -> ^(NAME coord)
	| NAME ':' NAME (',' NAME)* -> ^(NAME NAME+)
	) ';'
	;

settings: 'Settings' '(' settingsRow+ ')' -> ^(SETTINGS settingsRow+);

pieceStartingCoords:
	NAME ':' 'Offboard' ';' -> ^(NAME 'Offboard') |
	NAME ':' coord ';' -> ^(NAME coord) |
	NAME NAME ':' 'Offboard' ';' -> ^(NAME 'Offboard' ^(TAG NAME)) |
	NAME NAME ':' coord ';' -> ^(NAME coord ^(TAG NAME)) ;

startingBoardRow:
	(
		playerRef '(' pieceStartingCoords+ ')' -> ^(STARTINGPIECES playerRef ^(LIST pieceStartingCoords+))
	)
	;

invalidBoardRow:
	(
		'Valid' '(' (expr ';')+ ')' -> ^('Valid' expr+ ) |
		'Invalid' '(' (expr ';')+ ')' -> ^('Invalid' expr+ )
	);

startingBoard: 'StartingBoard' '(' invalidBoardRow? startingBoardRow+ ')' -> ^(STARTINGBOARD invalidBoardRow? startingBoardRow+);

coordOffboard: coord | OFFBOARD;

moveIf: 'If' expr -> ^(IF expr);

moveThen:
	'Then' statement+ 'End' -> ^(STATEMENTS statement+);

moveOp:
	from=coordOffboard '->' 'Empty'? to=coordOffboard moveIf? moveThen?
	->
	^( OP_MOVE ^(MOVE_FROM $from) ^(MOVE_TO $to) ^(MOVE_OPTIONS 'Empty'?) moveIf? moveThen? );

moveRow:
	NAME ':' moveOp ';' -> ^(NAME moveOp);

moves: 'Moves' '(' moveRow+ ')' -> ^(MOVES moveRow+);

eventType: 'Only'? playerRef '.' NAME -> ^(EVENTTYPE playerRef NAME ONLY_MODIFIER);

event: eventType ( ',' eventType )* '(' statement+ ')' -> ^(EVENT ^(EVENTTYPES eventType+)  ^(STATEMENTS statement+) );

events: 'Events' '(' event+ ')' -> ^(EVENTS event+);

init: 'Init' '(' statement+ ')' -> ^(INIT ^(STATEMENTS statement+));

funcDef:
	name '(' ( NAME (',' NAME)*)? ')' '(' statement+ ')' -> ^(FUNCDEF name ^(PARAMLIST NAME*) ^(STATEMENTS statement+));

functionBlock:
	'Functions' '(' funcDef+ ')' -> ^(FUNCDEFLIST funcDef+);

sentence: settings functionBlock? init? startingBoard? moves events EOF;
