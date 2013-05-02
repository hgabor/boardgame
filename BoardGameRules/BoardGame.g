grammar BoardGame;

options {
    language=CSharp3;
    output=AST;
	backtrack=true;
}

tokens {
	EVENT;
	EVENTS;
	EVENTTYPE;
	EVENTTYPES;
	FUNCCALL;
	IF;
	IF_CONDITION;
	IF_ACTION;
	LIST;
	LIT_COORDS;
	LIT_INT;
	LIT_SET;
	MEMBER_ACCESS;
	MOVE_FROM;
	MOVE_OPTIONS;
	MOVE_TO;
	MOVES;
	OP_MOVE;
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
}

@namespace { Level14.BoardGameRules }


// Lexer rules:

OP_ADD: '+';
OP_AND: 'And';
OP_DIV: '/';
OP_EQ: '=';
OP_GT: '>';
OP_GTE: '>=';
OP_LT: '<';
OP_LTE: '<=';
OP_MOD: '%';
OP_MUL: '*';
OP_NE: '!=';
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
name: NAME -> ^(REF NAME);

ref:
	  name '.' ref -> ^(MEMBER_ACCESS name ref)
	| playerRef
	| functionCall
	| name;

intRef: int | ref;

functionCall: name '(' expr? (',' expr)* ')' -> ^(FUNCCALL name ^(LIST expr*) );

namelist_comma: NAME (',' NAME) -> NAME+;


// Expressions:

coord: '{' placeholderExpr (',' placeholderExpr )* '}' -> ^(LIT_COORDS placeholderExpr+);
playerRef: 'Player' '(' expr ')' -> ^(PLAYERREF expr);

selectExpr: '[' 'Select' name 'From' ref ( 'Where' expr )? ']' -> ^(SELECT name ^(SELECT_FROM ref) ^(SELECT_WHERE expr));
setLiteral: '[' (expr (',' expr)* ','?)? ']' -> ^(LIT_SET expr+);
setExpr: selectExpr | setLiteral;

expr: andExpr (OP_OR andExpr)*;
andExpr: relExpr (OP_AND^ relExpr)*;
relExpr: addExpr ((OP_EQ|OP_NE|OP_GT|OP_LT|OP_GTE|OP_LTE)^ addExpr)*;
addExpr: mulExpr ((OP_ADD|OP_SUB)^ mulExpr)*;
mulExpr: signExpr ((OP_MUL|OP_DIV|OP_MOD)^ signExpr)*;
signExpr:
	OP_SUB primeExpr -> ^(OP_SUB primeExpr) |
	OP_ADD? primeExpr -> primeExpr;

primeExpr: int | coord | ref | setExpr | '(' expr ')';
placeholderExpr: '_' | expr;

ifBlock:
	'If' expr 'Then' statement+ 'End' -> ^(IF ^(IF_CONDITION expr) ^(IF_ACTION statement+) );

statement:
	functionCall ';' -> functionCall |
	ifBlock;
settingsRow:
	(
	  NAME ':' int -> ^(NAME int)
	| NAME ':' coord -> ^(NAME coord)
	| NAME ':' namelist_comma -> ^(NAME namelist_comma)
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
		'Invalid' '(' (expr ';')+ ')'
	);

startingBoard: 'StartingBoard' '(' invalidBoardRow? startingBoardRow+ ')' -> ^(STARTINGBOARD startingBoardRow+);

coordOffboard: coord | OFFBOARD;

moveThen:
	'Then' statement+ 'End' -> ^(STATEMENTS statement+);

moveOp:
	from=coordOffboard '->' 'Empty'? to=coordOffboard moveThen?
	->
	^( OP_MOVE ^(MOVE_FROM $from) ^(MOVE_TO $to) ^(MOVE_OPTIONS 'Empty'?) moveThen? );

moveRow:
	NAME ':' moveOp ';' -> ^(NAME moveOp);

moves: 'Moves' '(' moveRow+ ')' -> ^(MOVES moveRow+);

eventType: playerRef '.' NAME -> ^(EVENTTYPE playerRef NAME);

event: eventType ( ',' eventType )* '(' statement+ ')' -> ^(EVENT ^(EVENTTYPES eventType+)  ^(STATEMENTS statement+) );

events: 'Events' '(' event+ ')' -> ^(EVENTS event+);

sentence: settings startingBoard? moves events EOF;
