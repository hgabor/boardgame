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
	MEMBER_ACCESS;
	MOVE_FROM;
	MOVE_OPTIONS;
	MOVE_TO;
	MOVES;
	OP_MOVE;
	PLAYERREF;
	REF;
	SETTINGS;
	STARTINGPIECES;
	STARTINGBOARD;
	STATEMENTS;
}

@namespace { Level14.BoardGameRules }


// Lexer rules:

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
	| functionCall
	| name;

intRef: int | ref;

functionCall: name '(' expr? (',' expr)* ')' -> ^(FUNCCALL name ^(LIST expr*) );

namelist_comma: NAME (',' NAME) -> NAME+;

addExpr: intRef '+' intExpr -> ^('+' intRef intExpr );
subExpr: intRef '-' intExpr -> ^('-' intRef intExpr );
intExpr: addExpr | subExpr | intRef;
intExprP: intExpr | PLACEHOLDER;

gteExpr: intExpr '>=' intExpr -> ^('>=' intExpr+);
boolExpr: gteExpr | functionCall;

coord: '{' intExprP (',' intExprP )* '}' -> ^(LIT_COORDS intExprP+);
playerRef: 'Player' '(' intExpr ')' -> ^(PLAYERREF intExpr);

expr:
	  playerRef
	| coord
	| boolExpr
	| intExpr;


ifBlock:
	'If' boolExpr 'Then' statement+ 'End' -> ^(IF ^(IF_CONDITION boolExpr) ^(IF_ACTION statement+) );

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

pieceStartingCoords: NAME ':' coord ';' -> ^(NAME coord);

startingBoardRow:
	(
		playerRef '(' pieceStartingCoords+ ')' -> ^(STARTINGPIECES playerRef ^(LIST pieceStartingCoords+))
	)
	;

startingBoard: 'StartingBoard' '(' startingBoardRow+ ')' -> ^(STARTINGBOARD startingBoardRow+);

moveOp:
	coord '->' 'Empty'? coord -> ^( OP_MOVE ^(MOVE_FROM coord) ^(MOVE_TO coord) ^(MOVE_OPTIONS 'Empty'?) );

moveRow:
	NAME ':' moveOp ';' -> ^(NAME moveOp);

moves: 'Moves' '(' moveRow+ ')' -> ^(MOVES moveRow+);

eventType: playerRef '.' NAME -> ^(EVENTTYPE playerRef NAME);

event: eventType ( ',' eventType )* '(' statement+ ')' -> ^(EVENT ^(EVENTTYPES eventType+)  ^(STATEMENTS statement+) );

events: 'Events' '(' event+ ')' -> ^(EVENTS event+);

sentence: settings startingBoard? moves events EOF;
